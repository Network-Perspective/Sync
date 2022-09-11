using NetworkPerspective.Sync.Application.Domain.StatusLogs;
using NetworkPerspective.Sync.Framework.Dtos;

namespace NetworkPerspective.Sync.Framework.Mappers
{
    public static class StatusLogMapper
    {
        public static StatusLogDto DomainStatusLogToDto(StatusLog status)
        {
            return new StatusLogDto
            {
                TimeStamp = status.TimeStamp,
                Message = status.Message,
                Level = DomainLevelToDtoLevel(status.Level)
            };
        }

        private static StatusLogLevelDto DomainLevelToDtoLevel(StatusLogLevel type)
        {
            switch (type)
            {
                case StatusLogLevel.Error:
                    return StatusLogLevelDto.Error;
                case StatusLogLevel.Warning:
                    return StatusLogLevelDto.Warning;
                case StatusLogLevel.Info:
                    return StatusLogLevelDto.Info;
                default:
                    return StatusLogLevelDto.Info;
            }
        }
    }
}