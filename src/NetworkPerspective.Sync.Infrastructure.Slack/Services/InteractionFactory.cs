using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
    internal class InteractionFactory
    {
        private readonly HashFunction _hash;
        private readonly EmployeeCollection _employeeLookupTable;

        public InteractionFactory(HashFunction hash, EmployeeCollection employeeLookupTable)
        {
            _hash = hash;
            _employeeLookupTable = employeeLookupTable;
        }

        public ISet<Interaction> CreateFromThreadMessage(ConversationHistoryResponse.SingleMessage threadMessage, string channelId, ISet<string> channelMembers)
        {
            var interactions = new HashSet<Interaction>(new InteractionEqualityComparer());
            var threadId = threadMessage.MessageId;
            var timeStamp = TimeStampMapper.SlackTimeStampToDateTime(threadMessage.TimeStamp);

            foreach (var channelMember in channelMembers)
            {
                var interaction = Interaction.CreateChatThread(
                    timestamp: timeStamp,
                    source: _employeeLookupTable.Find(threadMessage.User),
                    target: _employeeLookupTable.Find(channelMember),
                    channelId: channelId,
                    eventId: threadId);

                interactions.Add(interaction.Hash(_hash));
            }

            var reactingUsers = threadMessage.Reactions.SelectMany(x => x.Users);
            foreach (var reactingUser in reactingUsers)
            {
                var reactionHash = HashCode.Combine(timeStamp, reactingUser, channelId);

                var interaction = Interaction.CreateChatReaction(
                    timestamp: timeStamp,
                    source: _employeeLookupTable.Find(reactingUser),
                    target: _employeeLookupTable.Find(threadMessage.User),
                    channelId: channelId,
                    eventId: threadId + reactionHash.ToString(),
                    parentEventId: threadId);

                interactions.Add(interaction.Hash(_hash));
            }

            return interactions;
        }

        public ISet<Interaction> CreateFromThreadReplies(IEnumerable<ConversationRepliesResponse.SingleMessage> replies, string channelId, string threadId, string threadCreator, TimeRange timeRange)
        {
            var interactions = new HashSet<Interaction>(new InteractionEqualityComparer());
            var activeUsers = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { threadCreator };

            foreach (var reply in replies)
            {
                var replyId = reply.MessageId;
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
                        var reactionHash = HashCode.Combine(timeStamp, reactingUser, channelId, replyId);

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