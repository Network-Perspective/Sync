using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Configs;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Configs
{
    internal class SlackConfig
    {
        public AuthConfig Auth { get; set; }
        public string BaseUrl { get; set; }
        public Resiliency Resiliency { get; set; }
    }
}