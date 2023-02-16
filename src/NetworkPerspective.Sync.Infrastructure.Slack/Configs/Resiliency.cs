using System;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Configs
{
    internal sealed class Resiliency
    {
        public TimeSpan[] Retries { get; set; } = Array.Empty<TimeSpan>();
    }
}