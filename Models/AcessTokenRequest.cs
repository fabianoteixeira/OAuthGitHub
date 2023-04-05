namespace GitHubOAuth.Models
{
    public class AcessTokenRequest
    {
        public string client_id { get; set; }
        public string client_secret { get; set; }
        public string code { get; set; }
    }
}
