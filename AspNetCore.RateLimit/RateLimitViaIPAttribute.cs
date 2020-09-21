using BigEgg.AspNetCore.RateLimit.Filter;
using Microsoft.AspNetCore.Mvc;
using System;

namespace BigEgg.AspNetCore.RateLimit
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RateLimitViaIPAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimitViaIPAttribute" /> class.
        /// </summary>
        /// <param name="limit">The maximum number of requests that a client can make in a defined period.</param>
        /// <param name="period">The rate limit period (in seconds).</param>
        /// <exception cref="ArgumentException">
        /// limit - Limit should not less than zero.
        /// or
        /// period - Period should not less than zero.
        /// </exception>
        public RateLimitViaIPAttribute(int limit, int period)
            : base(typeof(RateLimitActionFilter))
        {
            if (limit < 0)
            {
                throw new ArgumentException("Limit should not less than zero.", nameof(limit));
            }

            if (period < 0)
            {
                throw new ArgumentException("Period should not less than zero.", nameof(period));
            }

            Arguments = new object[] { new RateLimitArgument(RateLimitType.ViaIP, limit, period) };
        }

        /// <summary>
        /// Gets the maximum number of requests that a client can make in a defined period.
        /// </summary>
        /// <value>
        /// The maximum number of requests that a client can make in a defined period.
        /// </value>
        public int Limit { get; }

        /// <summary>
        /// Gets the rate limit period.
        /// </summary>
        /// <value>
        /// The rate limit period (in seconds).
        /// </value>
        public int Period { get; }
    }
}
