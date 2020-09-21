using BigEgg.AspNetCore.RateLimit.Models;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BigEgg.AspNetCore.RateLimit.Stores
{
    public class MemoryCacheRateLimitCounterStore : IRateLimitCounterStore
    {
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheRateLimitCounterStore"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        public MemoryCacheRateLimitCounterStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_cache.TryGetValue(id, out _));
        }

        public Task<RateLimitCounter> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(id, out RateLimitCounter stored))
            {
                return Task.FromResult(stored);
            }

            return Task.FromResult((RateLimitCounter)null);
        }

        public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            _cache.Remove(id);

            return Task.CompletedTask;
        }

        public Task SetAsync(string id, RateLimitCounter entry, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
        {
            var options = new MemoryCacheEntryOptions
            {
                Priority = CacheItemPriority.NeverRemove,
            };

            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }

            _cache.Set(id, entry, options);

            return Task.CompletedTask;
        }
    }
}
