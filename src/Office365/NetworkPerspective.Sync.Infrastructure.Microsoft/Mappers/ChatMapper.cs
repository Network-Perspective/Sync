using System.Linq;

using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Infrastructure.Microsoft.Extensions;

using InternalChat = NetworkPerspective.Sync.Infrastructure.Microsoft.Models.Chat;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Mappers
{
    internal static class ChatMapper
    {
        public static InternalChat ToInternalChat(Chat chat)
        {
            var members = chat.Members.Select(x => x.GetUserId());
            return new InternalChat(chat.Id, members);
        }
    }
}