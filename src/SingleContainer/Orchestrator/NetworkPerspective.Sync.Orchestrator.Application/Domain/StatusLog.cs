using System;

using NetworkPerspective.Sync.Application.Domain.Statuses;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class StatusLog
{
    public Guid DataSourceId { get; }
    public DateTime TimeStamp { get; }
    public string Message { get; }
    public StatusLogLevel Level { get; }

    private StatusLog(Guid dataSourceId, string message, StatusLogLevel level, DateTime timestamp)
    {
        DataSourceId = dataSourceId;
        Message = message;
        Level = level;
        TimeStamp = timestamp;
    }

    public static StatusLog Create(Guid dataSourceId, string message, StatusLogLevel level, DateTime timestamp)
        => new StatusLog(dataSourceId, message, level, timestamp);
}