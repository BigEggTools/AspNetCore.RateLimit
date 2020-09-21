using BigEgg.AspNetCore.RateLimit.Filter;
using Microsoft.AspNetCore.Mvc;
using System;

namespace BigEgg.AspNetCore.RateLimit
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RateLimitViaParameterAttribute : TypeFilterAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RateLimitViaParameterAttribute"/> class.
        /// </summary>
        /// <param name="names">The parameter names (split via ';').</param>
        /// <param name="limit">The maximum number of requests that a client can make in a defined period.</param>
        /// <param name="period">The rate limit period (in seconds).</param>
        /// <exception cref="ArgumentNullException">names - Parameter name should not be empty or white space.</exception>
        /// <exception cref="ArgumentException">
        /// Limit should not less than zero. - limit
        /// or
        /// Period should not less than zero. - period
        /// </exception>
        public RateLimitViaParameterAttribute(string names, int limit, int period)
            : base(typeof(RateLimitActionFilter))
        {
            if (string.IsNullOrWhiteSpace(names))
            {
                throw new ArgumentNullException(nameof(names), "Parameter name should not be empty or white space.");
            }

            if (limit < 0)
            {
                throw new ArgumentException("Limit should not less than zero.", nameof(limit));
            }

            if (period < 0)
            {
                throw new ArgumentException("Period should not less than zero.", nameof(period));
            }

            Arguments = new object[] { new RateLimitArgument(RateLimitType.ViaParameter, limit, period, names) };
        }

        /// <summary>
        /// Gets the parameter names.
        /// </summary>
        /// <value>
        /// The parameter names (split via ';').
        /// </value>
        public string Names { get; }

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
