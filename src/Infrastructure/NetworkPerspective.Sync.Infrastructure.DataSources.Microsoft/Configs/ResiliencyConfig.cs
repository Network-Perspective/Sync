using System;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Configs;

internal sealed class ResiliencyConfig
{
    public TimeSpan[] Retries { get; set; } = [];
}