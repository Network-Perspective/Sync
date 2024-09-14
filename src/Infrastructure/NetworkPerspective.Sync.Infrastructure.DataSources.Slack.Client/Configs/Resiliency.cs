using System;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Configs
{
    public sealed class Resiliency
    {
        public TimeSpan[] Retries { get; set; } = Array.Empty<TimeSpan>();
    }
}