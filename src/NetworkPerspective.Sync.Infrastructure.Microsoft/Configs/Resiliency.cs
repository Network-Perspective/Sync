using System;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Configs
{
    internal sealed class Resiliency
    {
        public TimeSpan[] Retries { get; set; } = Array.Empty<TimeSpan>();
    }
}
