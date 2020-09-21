using System;

namespace BigEgg.AspNetCore.RateLimit.Models
{
    /// <summary>
    /// Stores the initial access time and the numbers of calls made from that point
    /// </summary>
    public class RateLimitCounter
    {
        /// <summary>
        /// Gets or sets the initial access timestamp.
        /// </summary>
        /// <value>
        /// The initial access timestamp.
        /// </value>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the numbers of calls made from the initial access timestamp.
        /// </summary>
        /// <value>
        /// The numbers of calls made from the initial access timestamp.
        /// </value>
        public double Count { get; set; }
    }
}
