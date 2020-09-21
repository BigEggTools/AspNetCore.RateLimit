using BigEgg.AspNetCore.RateLimit.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BigEgg.AspNetCore.RateLimit.Stores
{
    public interface IRateLimitCounterStore
    {
        Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);

        Task<RateLimitCounter> GetAsync(string id, CancellationToken cancellationToken = default);

        Task RemoveAsync(string id, CancellationToken cancellationToken = default);

        Task SetAsync(string id, RateLimitCounter entry, TimeSpan? expirationTime = null, CancellationToken cancellationToken = default);
    }
}
