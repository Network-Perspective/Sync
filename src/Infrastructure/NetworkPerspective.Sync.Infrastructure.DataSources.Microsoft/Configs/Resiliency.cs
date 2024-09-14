using System;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Configs
{
    internal sealed class Resiliency
    {
        public TimeSpan[] Retries { get; set; } = Array.Empty<TimeSpan>();
    }
}