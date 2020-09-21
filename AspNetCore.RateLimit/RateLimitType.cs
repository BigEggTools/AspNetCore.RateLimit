namespace BigEgg.AspNetCore.RateLimit
{
    /// <summary>
    /// The rate limit type
    /// </summary>
    public enum RateLimitType
    {
        ViaIP,
        ViaClientID,
        ViaParameter,
    }
}
