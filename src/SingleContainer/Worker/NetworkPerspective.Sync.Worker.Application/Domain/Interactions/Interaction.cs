﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;

using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Meetings;
using NetworkPerspective.Sync.Worker.Application.Exceptions;
using NetworkPerspective.Sync.Worker.Application.Extensions;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Interactions
{
    [DebuggerDisplay("{Source.Id.PrimaryId} -> {Target.Id.PrimaryId}")]
    public class Interaction
    {
        public string Id { get; }
        public bool IsHashed { get; }
        public DateTime Timestamp { get; }
        public Employee Source { get; }
        public Employee Target { get; }
        public InteractionType Type { get; }
        public string ChannelId { get; }
        public string ParentEventId { get; }
        public string EventId { get; }
        public RecurrenceType? Recurring { get; }
        public ISet<UserActionType> UserAction { get; }
        public int? Duration { get; }

        private Interaction(string id, DateTime timestamp, Employee source, Employee target, InteractionType type, string channelId, string eventId, string parentEventId, RecurrenceType? recurring, ISet<UserActionType> userActions, int? duration, bool isHashed)
        {
            Id = id;
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
                id: (timestamp.Ticks.ToString() + source.Id.PrimaryId + target.Id.PrimaryId + eventId).GetMd5HashCode(),
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
                id: (timestamp.Ticks.ToString() + source.Id.PrimaryId + target.Id.PrimaryId + eventId).GetMd5HashCode(),
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
                id: (timestamp.Ticks.ToString() + source.Id.PrimaryId + target.Id.PrimaryId + eventId).GetMd5HashCode(),
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

        public Interaction Hash(HashFunction.Delegate hashFunc)
        {
            if (IsHashed)
                throw new DoubleHashingException(nameof(Interaction));

            return new Interaction(
                id: Id,
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