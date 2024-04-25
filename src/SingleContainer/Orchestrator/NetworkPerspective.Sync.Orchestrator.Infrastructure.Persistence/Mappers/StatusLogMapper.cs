using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Persistence.Mappers;

public static class StatusLogMapper
{
    public static StatusLog EntityToDomainModel(StatusLogEntity entity)
        => StatusLog.Create(entity.DataSourceId, entity.Message, (StatusLogLevel)entity.Level, entity.TimeStamp);

    public static StatusLogEntity DomainModelToEntity(StatusLog log)
    {
        return new StatusLogEntity
        {
            DataSourceId = log.DataSourceId,
            TimeStamp = log.TimeStamp,
            Level = (int)log.Level,
            Message = log.Message
        };
    }
}