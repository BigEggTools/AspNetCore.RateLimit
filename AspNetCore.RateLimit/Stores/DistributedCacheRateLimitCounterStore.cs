using BigEgg.AspNetCore.RateLimit.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BigEgg.AspNetCore.RateLimit.Stores
{
    public class DistributedCacheRateLimitCounterStore : IRateLimitCounterStore
    {
        private readonly IDistributedCache _cache;

        /// <summary>
        /// Initializes a new instance of the <see cref="DistributedCacheRateLimitCounterStore"/> class.
        /// </summary>
        /// <param name="cache">The cache.</param>
        public DistributedCacheRateLimitCounterStore(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default)
        {
            var stored = await _cache.GetStringAsync(id, cancellationToken);

            return !string.IsNullOrEmpty(stored);
        }

        public async Task<RateLimitCounter> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            var stored = await _cache.GetStringAsync(id, cancellationToken);

            if (!string.IsNullOrEmpty(stored))
            {
                return JsonConvert.DeserializeObject<RateLimitCounter>(stored);
            }

            return null;
        }

        public Task RemoveAsync(string id, CancellationToken cancellationToken = default)
        {
            return _cache.RemoveAsync(id, cancellationToken);
        }

        public Task SetAsync(string id, RateLimitCounter entry, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default)
        {
            var options = new DistributedCacheEntryOptions();

            if (expirationTime.HasValue)
            {
                options.SetAbsoluteExpiration(expirationTime.Value);
            }

            return _cache.SetStringAsync(id, JsonConvert.SerializeObject(entry), options, cancellationToken);
        }
    }
}
