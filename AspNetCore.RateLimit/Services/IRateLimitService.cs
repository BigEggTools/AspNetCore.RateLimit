using BigEgg.AspNetCore.RateLimit.Models;
using System.Threading.Tasks;

namespace BigEgg.AspNetCore.RateLimit.Services
{
    public interface IRateLimitService
    {
        /// <summary>
        /// Processes the request asynchronous.
        /// </summary>
        /// <param name="identity">The request identity.</param>
        /// <param name="rule">The rate limit rule.</param>
        /// <returns><c>True</c> if request can be process, otherwise <c>false</c>.</returns>
        Task<bool> ProcessRequestAsync(RequestIdentity identity, RateLimitRule rule);

        /// <summary>
        /// Checks the rate limit availability asynchronous.
        /// </summary>
        /// <param name="identity">The request identity.</param>
        /// <param name="rule">The rate limit rule.</param>
        /// <returns>The availability.</returns>
        Task<int> CheckAvailabilityAsync(RequestIdentity identity, RateLimitRule rule);
    }
}
