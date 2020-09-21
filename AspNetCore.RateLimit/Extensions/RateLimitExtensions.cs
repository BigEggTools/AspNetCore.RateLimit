using BigEgg.AspNetCore.RateLimit.Services;
using BigEgg.AspNetCore.RateLimit.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace BigEgg.AspNetCore.RateLimit.Extensions
{
    public static class RateLimitExtensions
    {
        public static IServiceCollection SupportRateLimit(this IServiceCollection services, RateLimitStoreType storeType)
        {
            services.AddSingleton<IRateLimitService, RateLimitService>();
            if (storeType == RateLimitStoreType.MemoryCache)
            {
                services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            }
            else if (storeType == RateLimitStoreType.DistributedCache)
            {
                services.AddSingleton<IRateLimitCounterStore, DistributedCacheRateLimitCounterStore>();
            }

            return services;
        }

        public static IServiceCollection SupportRateLimit<TImplementation>(this IServiceCollection services) where TImplementation : class, IRateLimitCounterStore
        {
            services.AddSingleton<IRateLimitService, RateLimitService>();
            services.AddSingleton<IRateLimitCounterStore, TImplementation>();

            return services;
        }
    }
}
