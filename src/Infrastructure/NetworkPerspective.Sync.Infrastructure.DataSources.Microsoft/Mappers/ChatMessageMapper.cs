using System;

using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Exceptions;

using InternalChat = NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models.Chat;
using InternalChatMessage = NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models.ChatMessage;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Mappers
{
    internal static class ChatMessageMapper
    {
        public static InternalChatMessage ToInternalChatMessage(ChatMessage graphChatMessage, InternalChat internalChat)
        {
            var userId = GetUserId(graphChatMessage);
            var timestamp = GetTimestamp(graphChatMessage);
            return new InternalChatMessage(graphChatMessage.ChatId, userId, timestamp, internalChat);
        }

        private static string GetUserId(ChatMessage graphChatMessage)
        {
            if (graphChatMessage.From is null)
                throw new CannotEvaluateUserIdException($"{nameof(ChatMessage.From)} is null");

            if (graphChatMessage.From.User is null)
                throw new CannotEvaluateUserIdException($"{nameof(ChatMessageFromIdentitySet.User)} is null");

            if (graphChatMessage.From.User.Id is null)
                throw new CannotEvaluateUserIdException($"{nameof(Identity.Id)} is null");

            return graphChatMessage.From.User.Id;
        }

        private static DateTime GetTimestamp(ChatMessage graphChatMessage)
            => graphChatMessage.CreatedDateTime.HasValue
                ? graphChatMessage.CreatedDateTime.Value.DateTime
                : throw new CannotEveluateTimestampException($"{nameof(ChatMessage.CreatedDateTime)} has no value");
    }
}