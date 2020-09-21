using BigEgg.AspNetCore.RateLimit.Models;
using BigEgg.AspNetCore.RateLimit.Stores;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BigEgg.AspNetCore.RateLimit.Services
{
    public class RateLimitService : IRateLimitService, IDisposable
    {
        private const string Rate_Limit_Counter_Prefix = "rate_limit";
        private readonly ReaderWriterLockSlim _lockSlim = new ReaderWriterLockSlim();
        private readonly IRateLimitCounterStore _counterStore;
        private readonly ILogger<RateLimitService> _logger;
        private bool _disposed = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimitService"/> class.
        /// </summary>
        /// <param name="counterStore">The counter store.</param>
        /// <param name="logger">The logger.</param>
        public RateLimitService(IRateLimitCounterStore counterStore, ILogger<RateLimitService> logger)
        {
            _counterStore = counterStore;
            _logger = logger;
        }

        /// <summary>
        /// Processes the request asynchronous.
        /// </summary>
        /// <param name="identity">The request identity.</param>
        /// <param name="rule">The rate limit rule.</param>
        /// <returns>
        ///   <c>True</c> if request can be processed, otherwise <c>false</c>.
        /// </returns>
        public async Task<bool> ProcessRequestAsync(RequestIdentity identity, RateLimitRule rule)
        {
            if (rule.Limit <= 0)
            {
                _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Path} with identity {identity.Identity} has been blocked, quota {rule.Limit}/{rule.Period} exceeded.");
            }

            var now = DateTime.UtcNow;

            (var currentCounterId, string previousCounterId) = BuildCounterKey(identity, rule, now);

            _lockSlim.EnterUpgradeableReadLock();
            try
            {
                /*
                 * In here use 2 fixed window rate limit slots (current and previous) to simulate the sliding window.
                 * The computation should be:
                 *     TotalCount = CurrentSlotCount * Percentage + (PreviousSlotCount * (1 - Percentage )
                 * Where the percentage should be where time "now" in the current slot.
                 * So we have:
                 *     Percentage = (now - CurrentSlotStartTime) / Period
                 *
                 * With this computation if the total count is larger than the limit,
                 * that means the request should not be processed and should return "429 To Many Request"
                 *
                 * And the time slot is based on the time "now" and the Period, which is:
                 *     SlotId = (now's seconds from start) / Period
                 * This logic can be find in Function BuildCounterKey
                */

                var currentSlotCounter = await _counterStore.GetAsync(currentCounterId);
                if (currentSlotCounter == null || currentSlotCounter.Timestamp.AddSeconds(rule.Period) < now)
                {
                    // Entry had expired
                    currentSlotCounter = new RateLimitCounter { Timestamp = now, Count = 1, };
                }
                else
                {
                    // Quick return, when current slot is full, no need to compute with previous slot.
                    if (currentSlotCounter.Count >= rule.Limit)
                    {
                        _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Path} with identity {identity.Identity} has been blocked, quota {rule.Limit}/{rule.Period} reached in current slot.");
                        return false;
                    }

                    currentSlotCounter.Count++;
                }

                var previousSlotCounter = await _counterStore.GetAsync(previousCounterId);

                var totalCount = currentSlotCounter.Count;
                if (previousSlotCounter != null)
                {
                    var percentage = (now - currentSlotCounter.Timestamp).TotalSeconds / rule.Period;
                    totalCount = Math.Ceiling((currentSlotCounter.Count * percentage) + (previousSlotCounter.Count * (1 - percentage)));
                }

                if (totalCount > rule.Limit)
                {
                    _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Path} with identity {identity.Identity} has been blocked, quota {rule.Limit}/{rule.Period} exceeded by {totalCount} in current and previous slot.");
                    return false;
                }

                // Deep copy the counter
                var counter = new RateLimitCounter
                {
                    Timestamp = currentSlotCounter.Timestamp,
                    Count = currentSlotCounter.Count,
                };

                _lockSlim.EnterWriteLock();
                try
                {
                    // Cache the slot with longer expire time.
                    // For current slot remain time, it will be: TimeSpan.FromSeconds(rule.Period) - (now - currentSlotCounter.Timestamp)
                    // For previous slot, need another TimeSpan.FromSeconds(rule.Period)
                    // And in here add another TimeSpan.FromSeconds(rule.Period) to just let the cache live longer...
                    await _counterStore.SetAsync(currentCounterId, counter, TimeSpan.FromSeconds(rule.Period * 3) - (now - currentSlotCounter.Timestamp));
                }
                finally
                {
                    _lockSlim.ExitWriteLock();
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Request {identity.HttpVerb}:{identity.Path} with identity {identity.Identity} has caused a exception: ", ex);
                throw;
            }
            finally
            {
                _lockSlim.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Checks the rate limit availability asynchronous.
        /// </summary>
        /// <param name="identity">The request identity.</param>
        /// <param name="rule">The rate limit rule.</param>
        /// <returns>
        /// The availability.
        /// </returns>
        public async Task<int> CheckAvailabilityAsync(RequestIdentity identity, RateLimitRule rule)
        {
            if (rule.Limit <= 0)
            {
                return rule.Limit;
            }

            (var currentCounterId, _) = BuildCounterKey(identity, rule, DateTime.UtcNow);

            _lockSlim.EnterReadLock();
            try
            {
                var entry = await _counterStore.GetAsync(currentCounterId);
                if (entry == null)
                {
                    return rule.Limit;
                }
                else
                {
                    return (int)(rule.Limit - entry.Count);
                }
            }
            finally
            {
                _lockSlim.ExitReadLock();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);

            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_lockSlim != null)
                {
                    _lockSlim.Dispose();
                }
            }

            _disposed = true;
        }

        [SuppressMessage("Security", "CA5350:Do not use insecure cryptographic algorithm SHA1.", Justification = "Just need a hash value in here, it will not expose to client")]
        private (string current, string previous) BuildCounterKey(RequestIdentity identity, RateLimitRule rule, DateTime datetime)
        {
            var key = $"{identity.Identity}_{rule.Period}_{identity.HttpVerb}:{identity.Path}";

            var bytes = Encoding.UTF8.GetBytes(key);
            using (var algorithm = new SHA1Managed())
            {
                var hash = algorithm.ComputeHash(bytes);
                key = Convert.ToBase64String(hash);
            }

            var slotId = (int)(TimeSpan.FromTicks(datetime.Ticks).TotalSeconds / rule.Period);
            return ($"{Rate_Limit_Counter_Prefix}_{key}_{slotId}", $"{Rate_Limit_Counter_Prefix}_{key}_{slotId - 1}");
        }
    }
}
