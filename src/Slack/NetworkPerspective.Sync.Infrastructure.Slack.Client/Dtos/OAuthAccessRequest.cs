namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos
{
    public class OAuthAccessRequest
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Code { get; set; }
        public string GrantType { get; set; }
        public string RedirectUri { get; set; }
        public string RefreshToken { get; set; }
    }
}