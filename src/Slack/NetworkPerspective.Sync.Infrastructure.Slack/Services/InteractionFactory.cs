using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Combinations;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
    internal class InteractionFactory
    {
        private readonly HashFunction.Delegate _hash;
        private readonly EmployeeCollection _employeeLookupTable;

        public InteractionFactory(HashFunction.Delegate hash, EmployeeCollection employeeLookupTable)
        {
            _hash = hash;
            _employeeLookupTable = employeeLookupTable;
        }

        public ISet<Interaction> CreateFromThreadMessage(ConversationHistoryResponse.SingleMessage thread, string channelId, ISet<string> channelMembers)
        {
            if (thread.Subtype == "huddle_thread")
                return CreateFromHuddles(thread, channelId);
            else
                return CreateFromRegularThreadMessage(thread, channelId, channelMembers);
        }

        private ISet<Interaction> CreateFromHuddles(ConversationHistoryResponse.SingleMessage thread, string channelId)
        {
            var interactions = new HashSet<Interaction>(new InteractionEqualityComparer());
            var huddlesId = channelId + thread.TimeStamp;
            var participants = thread.VoiceChat.Participants
                .Where(x => !string.IsNullOrEmpty(x));

            var participantsCombinations = CombinationFactory<string>.CreateCombinations(participants);

            var start = TimeStampMapper.SlackTimeStampToDateTime(thread.VoiceChat.Start);
            var end = TimeStampMapper.SlackTimeStampToDateTime(thread.VoiceChat.End);
            var duration = (end - start).TotalMinutes;

            foreach (var combination in participantsCombinations)
            {
                var interaction = Interaction.CreateMeeting(
                    timestamp: start,
                    source: _employeeLookupTable.Find(combination.Source),
                    target: _employeeLookupTable.Find(combination.Target),
                    eventId: huddlesId,
                    recurring: null,
                    duration: (int)duration);

                interactions.Add(interaction.Hash(_hash));
            }

            return interactions;
        }

        private ISet<Interaction> CreateFromRegularThreadMessage(ConversationHistoryResponse.SingleMessage thread, string channelId, ISet<string> channelMembers)
        {
            if (thread.User is null)
                return ImmutableHashSet<Interaction>.Empty;

            var interactions = new HashSet<Interaction>(new InteractionEqualityComparer());
            var threadId = channelId + thread.TimeStamp;
            var timeStamp = TimeStampMapper.SlackTimeStampToDateTime(thread.TimeStamp);

            foreach (var channelMember in channelMembers)
            {
                var interaction = Interaction.CreateChatThread(
                    timestamp: timeStamp,
                    source: _employeeLookupTable.Find(thread.User),
                    target: _employeeLookupTable.Find(channelMember),
                    channelId: channelId,
                    eventId: threadId);

                interactions.Add(interaction.Hash(_hash));
            }

            var reactingUsers = thread.Reactions
                .SelectMany(x => x.Users)
                .Where(x => !string.IsNullOrEmpty(x));

            foreach (var reactingUser in reactingUsers)
            {
                var reactionHash = $"{timeStamp.Ticks}{reactingUser.GetStableHashCode()}{channelId.GetStableHashCode()}";

                var interaction = Interaction.CreateChatReaction(
                    timestamp: timeStamp,
                    source: _employeeLookupTable.Find(reactingUser),
                    target: _employeeLookupTable.Find(thread.User),
                    channelId: channelId,
                    eventId: threadId + reactionHash.ToString(),
                    parentEventId: threadId);

                interactions.Add(interaction.Hash(_hash));
            }

            return interactions;
        }

        public ISet<Interaction> CreateFromThreadReplies(IEnumerable<ConversationRepliesResponse.SingleMessage> replies, string channelId, string threadId, string threadCreator, TimeRange timeRange)
        {
            if (threadCreator is null)
                return ImmutableHashSet<Interaction>.Empty;

            var interactions = new HashSet<Interaction>(new InteractionEqualityComparer());
            var activeUsers = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { threadCreator };

            foreach (var reply in replies.Where(x => !string.IsNullOrEmpty(x.User)))
            {
                var replyId = threadId + reply.TimeStamp;
                var timeStamp = TimeStampMapper.SlackTimeStampToDateTime(reply.TimeStamp);

                foreach (var user in activeUsers)
                {
                    var interaction = Interaction.CreateChatReply(
                        timestamp: timeStamp,
                        source: _employeeLookupTable.Find(reply.User),
                        target: _employeeLookupTable.Find(user),
                        channelId: channelId,
                        eventId: replyId,
                        parentEventId: threadId);

                    if (timeRange.IsInRange(timeStamp))
                        interactions.Add(interaction.Hash(_hash));
                }

                activeUsers.Add(reply.User);

                if (timeRange.IsInRange(timeStamp))
                {
                    foreach (var reactingUser in reply.Reactions.SelectMany(x => x.Users))
                    {
                        var reactionHash = $"{timeStamp.Ticks}{reactingUser.GetStableHashCode()}{channelId.GetStableHashCode()}{replyId.GetStableHashCode()}";

                        var interaction = Interaction.CreateChatReaction(
                            timestamp: timeStamp,
                            source: _employeeLookupTable.Find(reactingUser),
                            target: _employeeLookupTable.Find(reply.User),
                            channelId: channelId,
                            eventId: replyId + reactionHash.ToString(),
                            parentEventId: replyId);

                        interactions.Add(interaction.Hash(_hash));
                    }
                }
            }

            return interactions;
        }
    }
}