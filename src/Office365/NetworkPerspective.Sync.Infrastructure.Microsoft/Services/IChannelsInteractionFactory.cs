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
        ISet<Interaction> CreateFromThreadMessage(ChatMessage thread, Models.Channel channel);
        ISet<Interaction> CreateFromThreadRepliesMessage(IEnumerable<ChatMessage> replies, string channelId, string threadId, string threadCreator, Application.Domain.TimeRange timeRange);
    }

    internal class ChannelInteractionFactory : IChannelsInteractionFactory
    {
        private readonly HashFunction.Delegate _hash;
        private readonly EmployeeCollection _employees;

        public ChannelInteractionFactory(HashFunction.Delegate hash, EmployeeCollection employees)
        {
            _hash = hash;
            _employees = employees;
        }

        public ISet<Interaction> CreateFromThreadMessage(ChatMessage thread, Models.Channel channel)
        {
            var interactions = new HashSet<Interaction>(new InteractionEqualityComparer());

            if (thread.From?.User?.Id is null)
                return ImmutableHashSet<Interaction>.Empty;

            foreach (var channelMember in channel.UserIds)
            {
                var interaction = Interaction.CreateChatThread(
                    timestamp: GetDateTimeOrMinValue(thread.CreatedDateTime),
                    source: _employees.Find(thread.From.User.Id),
                    target: _employees.Find(channelMember),
                    channelId: channel.Id,
                    eventId: thread.Id);

                interactions.Add(interaction.Hash(_hash));
            }

            var reactingUsers = thread
                .Reactions
                .Where(x => x.User?.User?.Id is not null)
                .SelectMany(x => x.User.User.Id)
                .ToList();

            foreach (var reaction in thread.Reactions.Where(x => x.CreatedDateTime.HasValue))
            {
                var reactionHash = $"{reaction.CreatedDateTime.Value.Ticks}{reaction.User.User.Id.GetStableHashCode()}{channel.Id.GetStableHashCode()}";

                var interaction = Interaction.CreateChatReaction(
                    timestamp: reaction.CreatedDateTime.Value.UtcDateTime,
                    source: _employees.Find(reaction.User.User.Id),
                    target: _employees.Find(thread.From.User.Id),
                    channelId: channel.Id,
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

            foreach (var reply in replies.Where(x => x.From?.User?.Id is not null))
            {
                foreach (var user in activeUsers)
                {
                    var interaction = Interaction.CreateChatReply(
                        timestamp: GetDateTimeOrMinValue(reply.CreatedDateTime),
                        source: _employees.Find(reply.From.User.Id),
                        target: _employees.Find(user),
                        channelId: channelId,
                        eventId: reply.Id,
                        parentEventId: threadId);

                    if (timeRange.IsInRange(reply.CreatedDateTime.Value.DateTime))
                        interactions.Add(interaction.Hash(_hash));
                }

                activeUsers.Add(reply.From.User.Id);

                var reactions = reply.Reactions
                    .Where(x => x.CreatedDateTime.HasValue)
                    .Where(x => x.User?.User?.Id is not null);

                foreach (var reaction in reactions)
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

        private static DateTime GetDateTimeOrMinValue(DateTimeOffset? input)
            => input.HasValue
                ? input.Value.UtcDateTime
                : DateTime.MinValue.ToUniversalTime();
    }
}