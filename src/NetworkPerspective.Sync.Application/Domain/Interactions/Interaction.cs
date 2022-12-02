using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Meetings;
using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Extensions;

namespace NetworkPerspective.Sync.Application.Domain.Interactions
{
    public class Interaction
    {
        public bool IsHashed { get; init; }
        public DateTime Timestamp { get; init; }
        public Employee Source { get; init; }
        public Employee Target { get; init; }
        public InteractionType Type { get; init; }
        public string ChannelId { get; init; }
        public string ParentEventId { get; init; }
        public string EventId { get; init; }
        public RecurrenceType? Recurring { get; init; }
        public ISet<UserActionType> UserAction { get; init; }
        public int? Duration { get; init; }

        public Interaction()
        {

        }

        private Interaction(DateTime timestamp, Employee source, Employee target, InteractionType type, string channelId, string eventId, string parentEventId, RecurrenceType? recurring, ISet<UserActionType> userActions, int? duration, bool isHashed)
        {
            Timestamp = timestamp;
            Source = source;
            Target = target;
            Type = type;
            ChannelId = channelId;
            EventId = eventId;
            ParentEventId = parentEventId;
            Recurring = recurring;
            UserAction = userActions;
            Duration = duration;
            IsHashed = isHashed;
        }

        public static Interaction CreateEmail(DateTime timestamp, Employee source, Employee target, string eventId)
        {
            return new Interaction(
                timestamp: timestamp.Bucket(TimeSpan.FromMinutes(5)),
                source: source,
                target: target,
                type: InteractionType.Email,
                eventId: eventId,
                parentEventId: null,
                userActions: ImmutableHashSet<UserActionType>.Empty,
                recurring: null,
                channelId: null,
                duration: null,
                isHashed: false);
        }

        public static Interaction CreateMeeting(DateTime timestamp, Employee source, Employee target, string eventId, RecurrenceType? recurring, int duration)
        {
            return new Interaction(
                timestamp: timestamp.Bucket(TimeSpan.FromMinutes(5)),
                source: source,
                target: target,
                type: InteractionType.Meetings,
                channelId: null,
                eventId: eventId,
                parentEventId: null,
                recurring: recurring,
                userActions: ImmutableHashSet<UserActionType>.Empty,
                duration: duration,
                isHashed: false);
        }

        public static Interaction CreateChatThread(DateTime timestamp, Employee source, Employee target, string eventId, string channelId)
            => CreateChatInteraction(timestamp, source, target, eventId, null, channelId, new HashSet<UserActionType> { UserActionType.Thread });

        public static Interaction CreateChatReply(DateTime timestamp, Employee source, Employee target, string eventId, string parentEventId, string channelId)
            => CreateChatInteraction(timestamp, source, target, eventId, parentEventId, channelId, new HashSet<UserActionType> { UserActionType.Reply });

        public static Interaction CreateChatReaction(DateTime timestamp, Employee source, Employee target, string eventId, string parentEventId, string channelId)
            => CreateChatInteraction(timestamp, source, target, eventId, parentEventId, channelId, new HashSet<UserActionType> { UserActionType.Reaction });

        private static Interaction CreateChatInteraction(DateTime timestamp, Employee source, Employee target, string eventId, string parentEventId, string channelId, ISet<UserActionType> userActionType)
        {
            return new Interaction(
                timestamp: timestamp.Bucket(TimeSpan.FromMinutes(5)),
                source: source,
                target: target,
                type: InteractionType.Chat,
                channelId: channelId,
                eventId: eventId,
                parentEventId: parentEventId,
                recurring: null,
                userActions: userActionType,
                duration: null,
                isHashed: false);
        }

        public Interaction Hash(HashFunction hashFunc)
        {
            if (IsHashed)
                throw new DoubleHashingException(nameof(Interaction));

            return new Interaction(
                timestamp: Timestamp,
                source: Source.Hash(hashFunc),
                target: Target.Hash(hashFunc),
                type: Type,
                channelId: string.IsNullOrEmpty(ChannelId) ? ChannelId : hashFunc(ChannelId),
                eventId: string.IsNullOrEmpty(EventId) ? EventId : hashFunc(EventId),
                parentEventId: string.IsNullOrEmpty(ParentEventId) ? ParentEventId : hashFunc(ParentEventId),
                recurring: Recurring,
                userActions: UserAction,
                duration: Duration,
                isHashed: true);
        }
    }
}