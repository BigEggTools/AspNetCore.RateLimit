using System;

namespace BigEgg.AspNetCore.RateLimit.Filter
{
    public class RateLimitArgument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimitArgument"/> class.
        /// </summary>
        /// <param name="rateLimitType">Type of the rate limit.</param>
        /// <param name="limit">The maximum number of requests that a client can make in a defined period.</param>
        /// <param name="period">The rate limit period (in seconds).</param>
        /// <param name="metadata">The metadata.</param>
        /// <exception cref="ArgumentException">
        /// Limit should not less than zero. - limit
        /// or
        /// Period should not less than zero. - period
        /// </exception>
        public RateLimitArgument(RateLimitType rateLimitType, int limit, int period, string metadata = "")
        {
            if (limit < 0)
            {
                throw new ArgumentException("Limit should not less than zero.", nameof(limit));
            }

            if (period < 0)
            {
                throw new ArgumentException("Period should not less than zero.", nameof(period));
            }

            RateLimitType = rateLimitType;
            Limit = limit;
            Period = period;
            Metadata = metadata;
        }

        /// <summary>
        /// Gets the type of the rate limit.
        /// </summary>
        /// <value>
        /// The type of the rate limit.
        /// </value>
        public RateLimitType RateLimitType { get; private set; }

        /// <summary>
        /// Maximum number of requests that a client can make in a defined period
        /// </summary>
        public int Limit { get; private set; }

        /// <summary>
        /// Rate limit period in seconds
        /// </summary>
        public int Period { get; private set; }

        /// <summary>
        /// Gets the metadata.
        /// </summary>
        /// <value>
        /// The metadata.
        /// </value>
        public string Metadata { get; private set; }
    }
}
