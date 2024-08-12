using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models;
using NetworkPerspective.Sync.Worker.Application.Domain;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Interactions;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services
{
    internal interface IChatInteractionFactory
    {
        ISet<Interaction> CreateFromChatMessage(ChatMessage chatMessage);
        ISet<Interaction> CreateFromChatMessageReaction(ChatMessageReaction chatMessageReaction);
    }

    internal class ChatInteractionFactory : IChatInteractionFactory
    {
        private readonly HashFunction.Delegate _hash;
        private readonly EmployeeCollection _employees;

        public ChatInteractionFactory(HashFunction.Delegate hash, EmployeeCollection employees)
        {
            _hash = hash;
            _employees = employees;
        }

        public ISet<Interaction> CreateFromChatMessage(ChatMessage chatMessage)
        {
            var result = new HashSet<Interaction>(new InteractionEqualityComparer());

            foreach (var chatparticipant in chatMessage.Chat.UserIds.Where(x => x != chatMessage.SenderId))
            {
                var interaction = Interaction.CreateChatThread(
                    timestamp: chatMessage.TimeStamp.ToUniversalTime(),
                    source: _employees.Find(chatMessage.SenderId),
                    target: _employees.Find(chatparticipant),
                    channelId: chatMessage.Chat.Id,
                    eventId: chatMessage.Id);

                result.Add(interaction.Hash(_hash));
            }

            return result;
        }

        public ISet<Interaction> CreateFromChatMessageReaction(ChatMessageReaction chatMessageReaction)
        {
            var result = new HashSet<Interaction>(new InteractionEqualityComparer());

            var interaction = Interaction.CreateChatReaction(
                timestamp: chatMessageReaction.TimeStamp.ToUniversalTime(),
                source: _employees.Find(chatMessageReaction.SenderId),
                target: _employees.Find(chatMessageReaction.ChatMessage.SenderId),
                eventId: chatMessageReaction.Id,
                parentEventId: chatMessageReaction.ChatMessage.Id,
                channelId: chatMessageReaction.ChatMessage.Chat.Id);

            result.Add(interaction.Hash(_hash));

            return result;
        }
    }
}