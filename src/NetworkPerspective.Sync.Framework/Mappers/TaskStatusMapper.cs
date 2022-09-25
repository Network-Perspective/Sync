using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Framework.Dtos;

namespace NetworkPerspective.Sync.Framework.Mappers
{
    public static class TaskStatusMapper
    {
        public static TaskStatusDto DomainTaskStatusToDto(SynchronizationTaskStatus status)
        {
            return new TaskStatusDto
            {
                Caption = status.Caption,
                Description = status.Description,
                CompletionRate = status.CompletionRate
            };
        }
    }
}
