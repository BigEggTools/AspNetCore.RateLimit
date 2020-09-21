using BigEgg.AspNetCore.RateLimit.Filter;
using System;

namespace BigEgg.AspNetCore.RateLimit.Models
{
    public class RateLimitRule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimitRule"/> class.
        /// </summary>
        public RateLimitRule()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimitRule"/> class.
        /// </summary>
        /// <param name="argument">The argument.</param>
        /// <exception cref="ArgumentNullException">argument - Parameter argument cannot be null</exception>
        public RateLimitRule(RateLimitArgument argument)
        {
            if (argument == null)
            {
                throw new ArgumentNullException(nameof(argument), "Parameter argument cannot be null");
            }

            Period = argument.Period;
            Limit = argument.Limit;
        }

        /// <summary>
        /// Maximum number of requests that a client can make in a defined period
        /// </summary>
        public int Limit { get; set; }

        /// <summary>
        /// Rate limit period in seconds
        /// </summary>
        public int Period { get; set; }
    }
}
