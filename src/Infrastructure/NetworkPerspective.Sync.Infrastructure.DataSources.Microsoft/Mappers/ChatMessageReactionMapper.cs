using System;

using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Exceptions;
using NetworkPerspective.Sync.Worker.Application.Extensions;

using InternalChatMessage = NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models.ChatMessage;
using InternalChatMessageReaction = NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models.ChatMessageReaction;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Mappers
{
    internal static class ChatMessageReactionMapper
    {
        public static InternalChatMessageReaction ToInternalChatMessageReaction(ChatMessageReaction graphChatMessageReaction, InternalChatMessage internalChatMessage)
        {
            var userId = GetUserId(graphChatMessageReaction);
            var timestamp = GetTimestamp(graphChatMessageReaction);
            var id = $"{timestamp.Ticks}{userId.GetStableHashCode()}{internalChatMessage.Chat.Id.GetStableHashCode()}";

            return new InternalChatMessageReaction(id, userId, timestamp, internalChatMessage);
        }

        private static string GetUserId(ChatMessageReaction graphChatMessageReaction)
        {
            if (graphChatMessageReaction.User is null)
                throw new CannotEvaluateUserIdException($"{nameof(ChatMessageReaction.User)} is null");

            if (graphChatMessageReaction.User.User is null)
                throw new CannotEvaluateUserIdException($"{nameof(ChatMessageReactionIdentitySet.User)} is null");

            if (graphChatMessageReaction.User.User.Id is null)
                throw new CannotEvaluateUserIdException($"{nameof(Identity.Id)} is null");

            return graphChatMessageReaction.User.User.Id;
        }

        private static DateTime GetTimestamp(ChatMessageReaction graphChatMessageReaction)
            => graphChatMessageReaction.CreatedDateTime.HasValue
                ? graphChatMessageReaction.CreatedDateTime.Value.DateTime
                : throw new CannotEveluateTimestampException($"{nameof(ChatMessageReaction.CreatedDateTime)} has no value");
    }
}