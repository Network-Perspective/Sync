namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos
{
    internal class OAuthExchangeRequest
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string AccessToken { get; set; }
    }
}