namespace BigEgg.AspNetCore.RateLimit
{
    public static class RateLimitPeriod
    {
        public const int ONE_MINUTE = 60;
        public const int HALF_MINUTE = 30;

        public const int ONE_HOUR = 60 * ONE_MINUTE;
        public const int HALF_HOUR = 30 * ONE_MINUTE;

        public const int ONE_DAY = 24 * ONE_HOUR;
    }
}
