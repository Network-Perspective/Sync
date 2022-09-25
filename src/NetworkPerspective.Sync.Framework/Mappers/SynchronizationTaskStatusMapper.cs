using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Framework.Dtos;

namespace NetworkPerspective.Sync.Framework.Mappers
{
    public static class SynchronizationTaskStatusMapper
    {
        public static SynchronizationTaskStatusDto DomainTaskStatusToDto(SynchronizationTaskStatus status)
        {
            if (status == null)
                return null;

            return new SynchronizationTaskStatusDto
            {
                Caption = status.Caption,
                Description = status.Description,
                CompletionRate = status.CompletionRate
            };
        }
    }
}