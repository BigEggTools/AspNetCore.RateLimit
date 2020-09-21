namespace BigEgg.AspNetCore.RateLimit.Models
{
    /// <summary>
    /// Stores the client identity, endpoint and verb
    /// </summary>
    public class RequestIdentity
    {
        public string Identity { get; set; }

        public string Path { get; set; }

        public string HttpVerb { get; set; }
    }
}
