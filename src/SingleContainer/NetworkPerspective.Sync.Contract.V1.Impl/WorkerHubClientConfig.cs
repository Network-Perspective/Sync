using System;

namespace NetworkPerspective.Sync.Contract.V1.Impl;

public sealed class WorkerHubClientConfig
{
    public string BaseUrl { get; set; }
    public Resiliency Resiliency { get; set; }
}

public sealed class Resiliency
{
    public TimeSpan[] Retries { get; set; } = [];
}