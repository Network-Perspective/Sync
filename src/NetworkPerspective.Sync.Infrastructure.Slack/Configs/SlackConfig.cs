namespace NetworkPerspective.Sync.Infrastructure.Slack.Configs
{
    internal class SlackConfig
    {
        public AuthConfig Auth { get; set; }
        public string BaseUrl { get; set; }
        public Resiliency Resiliency { get; set; }
    }
}