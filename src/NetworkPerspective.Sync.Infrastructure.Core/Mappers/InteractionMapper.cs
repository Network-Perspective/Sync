using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Meetings;

namespace NetworkPerspective.Sync.Infrastructure.Core.Mappers
{
    internal static class InteractionMapper
    {
        public static HashedInteraction DomainIntractionToDto(Interaction domainInteraction, string dataSourceIdName)
        {
            var labels = new List<HashedInteractionLabel>();

            var recurrenceLabel = DomainRecurrenceTypeToLabel(domainInteraction.Recurring);
            if (recurrenceLabel != null)
                labels.Add((HashedInteractionLabel)recurrenceLabel);

            labels.Add(DomainInteractionTypeToLabel(domainInteraction.Type));

            labels.AddRange(DomainUserActionTypesToLabels(domainInteraction.UserAction));

            return new HashedInteraction
            {
                InteractionId = domainInteraction.Id,
                When = domainInteraction.Timestamp,
                SourceIds = IdsMapper.ToIds(domainInteraction.Source, dataSourceIdName),
                TargetIds = IdsMapper.ToIds(domainInteraction.Target, dataSourceIdName),
                EventId = domainInteraction.EventId,
                ParentEventId = domainInteraction.ParentEventId,
                ChannelId = domainInteraction.ChannelId,
                DurationMinutes = domainInteraction.Duration,
                Label = labels
            };
        }

        private static HashedInteractionLabel? DomainRecurrenceTypeToLabel(RecurrenceType? recurrenceType)
        {
            switch (recurrenceType)
            {
                case RecurrenceType.Yearly:
                    return HashedInteractionLabel.RecurringYearly;
                case RecurrenceType.Monthly:
                    return HashedInteractionLabel.RecurringMonthly;
                case RecurrenceType.Weekly:
                    return HashedInteractionLabel.RecurringWeekly;
                case RecurrenceType.Daily:
                    return HashedInteractionLabel.RecurringDaily;
                default:
                    return null;
            }
        }

        private static HashedInteractionLabel DomainInteractionTypeToLabel(InteractionType interactionType)
        {
            switch (interactionType)
            {
                case InteractionType.Chat:
                    return HashedInteractionLabel.Chat;
                case InteractionType.Meetings:
                    return HashedInteractionLabel.Meeting;
                case InteractionType.Email:
                    return HashedInteractionLabel.Email;
                default:
                    throw new ArgumentException($"Unable to map {nameof(InteractionType)} to {nameof(HashedInteractionLabel)}");
            }
        }

        private static IEnumerable<HashedInteractionLabel> DomainUserActionTypesToLabels(ISet<UserActionType> userActionTypes)
        {
            var labels = new List<HashedInteractionLabel>();

            foreach (var userActionType in userActionTypes)
            {
                if (userActionType == UserActionType.Thread)
                    labels.Add(HashedInteractionLabel.NewThread);
                else if (userActionType == UserActionType.Reply)
                    labels.Add(HashedInteractionLabel.Reply);
                else if (userActionType == UserActionType.Reaction)
                    labels.Add(HashedInteractionLabel.Reaction);
            }

            return labels;
        }
    }
}