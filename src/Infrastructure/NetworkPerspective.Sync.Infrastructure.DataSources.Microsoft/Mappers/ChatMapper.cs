using System.Linq;

using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Extensions;

using InternalChat = NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models.Chat;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Mappers
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