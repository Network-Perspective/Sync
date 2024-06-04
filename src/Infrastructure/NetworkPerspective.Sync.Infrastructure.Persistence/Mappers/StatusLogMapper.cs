﻿using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Infrastructure.Persistence.Entities;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Mappers
{
    public static class StatusLogMapper
    {
        public static StatusLog EntityToDomainModel(StatusLogEntity entity)
            => StatusLog.Create(entity.ConnectorId, entity.Message, (StatusLogLevel)entity.Level, entity.TimeStamp);

        public static StatusLogEntity DomainModelToEntity(StatusLog log)
        {
            return new StatusLogEntity
            {
                ConnectorId = log.ConnectorId,
                TimeStamp = log.TimeStamp,
                Level = (int)log.Level,
                Message = log.Message
            };
        }
    }
}