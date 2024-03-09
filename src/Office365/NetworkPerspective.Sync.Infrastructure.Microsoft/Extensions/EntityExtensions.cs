using Microsoft.Graph.Models;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Extensions
{
    internal static class EntityExtensions
    {
        public static string GetUserId(this Entity entity)
            => entity is AadUserConversationMember aadMember ? aadMember.Email : entity.Id;
    }
}