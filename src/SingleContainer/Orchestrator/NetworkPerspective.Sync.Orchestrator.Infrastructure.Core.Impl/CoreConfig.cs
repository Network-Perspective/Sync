using System;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Impl;

internal sealed class CoreConfig
{
    public string BaseUrl { get; set; }
    public string DataSourceIdName { get; set; }
    public int MaxInteractionsPerRequestCount { get; set; }
    public Resiliency Resiliency { get; set; }
}

internal sealed class Resiliency
{
    public TimeSpan[] Retries { get; set; } = Array.Empty<TimeSpan>();
}