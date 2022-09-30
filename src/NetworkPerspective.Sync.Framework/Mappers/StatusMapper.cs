using System.Linq;

using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Framework.Dtos;

namespace NetworkPerspective.Sync.Framework.Mappers
{
    public static class StatusMapper
    {
        public static StatusDto DomainStatusToDto(Status status)
        {
            return new StatusDto
            {
                Authorized = status.Authorized,
                Scheduled = status.Scheduled,
                Running = status.Running,
                CurrentTask = SynchronizationTaskStatusMapper.DomainTaskStatusToDto(status.CurrentTask),
                Logs = status.Logs.Select(StatusLogMapper.DomainStatusLogToDto)
            };
        }
    }
}