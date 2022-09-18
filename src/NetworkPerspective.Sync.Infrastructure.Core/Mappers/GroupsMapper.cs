using NetworkPerspective.Sync.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Infrastructure.Core.Mappers
{
    internal static class GroupsMapper
    {
        public static HashedGroup ToGroup(Group group)
        {
            return new HashedGroup
            {
                Id = group.Id,
                Name = group.Name,
                Category = group.Category.ToString(),
                ParentId = group.ParentId
            };
        }
    }
}