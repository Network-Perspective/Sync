using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Services
{
    internal interface IChannelsInteractionFactory
    {
        ISet<Interaction> CreateFromThreadMessage(ChatMessage thread, string channelId, ISet<string> channelMembers);
        ISet<Interaction> CreateFromThreadRepliesMessage(IEnumerable<ChatMessage> replies, string channelId, string threadId, string threadCreator, Application.Domain.TimeRange timeRange);
    }

    public class ChannelInteractionFactory : IChannelsInteractionFactory
    {
        private readonly HashFunction.Delegate _hash;
        private readonly EmployeeCollection _employees;

        public ChannelInteractionFactory(HashFunction.Delegate hash, EmployeeCollection employees)
        {
            _hash = hash;
            _employees = employees;
        }

        public ISet<Interaction> CreateFromThreadMessage(ChatMessage thread, string channelId, ISet<string> channelMembers)
        {
            var interactions = new HashSet<Interaction>(new InteractionEqualityComparer());

            foreach ( var channelMember in channelMembers)
            {
                var interaction = Interaction.CreateChatThread(
                    timestamp: thread.CreatedDateTime.Value.UtcDateTime,
                    source: _employees.Find(thread.From.User.Id),
                    target: _employees.Find(channelMember),
                    channelId: channelId,
                    eventId: thread.Id);

                interactions.Add(interaction.Hash(_hash));
            }

            var reactingUsers = thread
                .Reactions
                .SelectMany(x => x.User.User.Id)
                .ToList();

            foreach (var reaction in thread.Reactions)
            {
                var reactionHash = $"{reaction.CreatedDateTime.Value.Ticks}{reaction.User.User.Id.GetStableHashCode()}{channelId.GetStableHashCode()}";

                var interaction = Interaction.CreateChatReaction(
                    timestamp: reaction.CreatedDateTime.Value.UtcDateTime,
                    source: _employees.Find(reaction.User.User.Id),
                    target: _employees.Find(thread.From.User.Id),
                    channelId: channelId,
                    eventId: thread.Id + reactionHash.ToString(),
                    parentEventId: thread.Id);

                interactions.Add(interaction.Hash(_hash));
            }

            return interactions;
        }

        public ISet<Interaction> CreateFromThreadRepliesMessage(IEnumerable<ChatMessage> replies, string channelId, string threadId, string threadCreator, Application.Domain.TimeRange timeRange)
        {
            if (threadCreator is null)
                return ImmutableHashSet<Interaction>.Empty;

            var interactions = new HashSet<Interaction>(new InteractionEqualityComparer());
            var activeUsers = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { threadCreator };

            foreach (var reply in replies)
            {
                foreach(var user in activeUsers)
                {
                    var interaction = Interaction.CreateChatReply(
                        timestamp: reply.CreatedDateTime.Value.UtcDateTime,
                        source: _employees.Find(reply.From.User.Id),
                        target: _employees.Find(user),
                        channelId: channelId,
                        eventId: reply.Id,
                        parentEventId: threadId);

                    if (timeRange.IsInRange(reply.CreatedDateTime.Value.DateTime))
                        interactions.Add(interaction.Hash(_hash));
                }

                activeUsers.Add(reply.From.User.Id);


                foreach (var reaction in reply.Reactions)
                {
                    var reactionHash = $"{reaction.CreatedDateTime.Value.Ticks}{reaction.User.User.Id.GetStableHashCode()}{channelId.GetStableHashCode()}";

                    var interaction = Interaction.CreateChatReaction(
                        timestamp: reaction.CreatedDateTime.Value.UtcDateTime,
                        source: _employees.Find(reaction.User.User.Id),
                        target: _employees.Find(reply.From.User.Id),
                        channelId: channelId,
                        eventId: reply.Id + reactionHash.ToString(),
                        parentEventId: reply.Id);

                    interactions.Add(interaction.Hash(_hash));
                }
            }

            return interactions;
        }
    }
}
