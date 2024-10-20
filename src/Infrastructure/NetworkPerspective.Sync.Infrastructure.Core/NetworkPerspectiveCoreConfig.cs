using System;

namespace NetworkPerspective.Sync.Infrastructure.Core;

public sealed class NetworkPerspectiveCoreConfig
{
    public string BaseUrl { get; set; }
    public int MaxInteractionsPerRequestCount { get; set; }
    public Resiliency Resiliency { get; set; }
}

public sealed class Resiliency
{
    public TimeSpan[] Retries { get; set; } = [];
}