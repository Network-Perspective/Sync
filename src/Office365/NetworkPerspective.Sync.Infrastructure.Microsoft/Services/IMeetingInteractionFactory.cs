using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Meetings;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Services
{
    internal interface IMeetingInteractionFactory
    {
        ISet<Interaction> CreateForUser(Event @event, string userEmail);
    }

    internal class MeetingInteractionFactory : IMeetingInteractionFactory
    {
        private readonly HashFunction.Delegate _hashFunc;
        private readonly EmployeeCollection _employees;
        private readonly ILogger<MeetingInteractionFactory> _logger;

        public MeetingInteractionFactory(HashFunction.Delegate hashFunc, EmployeeCollection employees, ILogger<MeetingInteractionFactory> logger)
        {
            _hashFunc = hashFunc;
            _employees = employees;
            _logger = logger;
        }

        public ISet<Interaction> CreateForUser(Event @event, string userEmail)
        {
            var user = _employees.Find(userEmail);
            var duration = (@event.End.ToDateTimeOffset() - @event.Start.ToDateTimeOffset()).TotalMinutes;
            var timestamp = @event.Start.ToDateTimeOffset().DateTime;
            var participants = @event
                .Attendees
                .Where(x => x.EmailAddress?.Address is not null)        // Skip users without email address
                .Select(x => x.EmailAddress.Address)                    // Participants as email address 
                .Select(_employees.Find)                                // Map to Employee                   
                .Where(x => !Employee.EqualityComparer.Equals(x, user)) // Skip the user
                .Distinct(Employee.EqualityComparer);                   // Remove duplicates

            var recurrence = GetRecurrence(@event.Recurrence);

            if (IsSmallMeeting(participants.Count()))
                return CreateForSmallMeeting(@event.ICalUId, user, participants, timestamp, (int)duration, recurrence);
            else
                return CreateForBigMeeting(@event.ICalUId, user, participants, timestamp, (int)duration, recurrence);
        }

        private static RecurrenceType? GetRecurrence(PatternedRecurrence recurrence)
        {
            if (recurrence?.Pattern == null)
                return null;

            if (recurrence.Pattern.Type == RecurrencePatternType.Daily)
                return RecurrenceType.Daily;
            else if (recurrence.Pattern.Type == RecurrencePatternType.Weekly)
            {
                if (recurrence.Pattern.DaysOfWeek.Count == 1)
                    return RecurrenceType.Weekly;
                else
                    return RecurrenceType.Daily;
            }
            else
                return null;
        }

        private ISet<Interaction> CreateForSmallMeeting(string eventId, Employee user, IEnumerable<Employee> participants, DateTime timestamp, int duration, RecurrenceType? recurrence)
        {
            var result = new HashSet<Interaction>(new InteractionEqualityComparer());

            foreach (var participant in participants)
            {
                var outgoingInteraction = Interaction.CreateMeeting(
                    timestamp: timestamp,
                    source: user,
                    target: participant,
                    eventId: eventId,
                    recurring: recurrence,
                    duration: duration);

                result.Add(outgoingInteraction.Hash(_hashFunc));

                if (participant.IsExternal)
                {
                    var incomingInteraction = Interaction.CreateMeeting(
                        timestamp: timestamp,
                        source: participant,
                        target: user,
                        eventId: eventId,
                        recurring: recurrence,
                        duration: duration);

                    result.Add(incomingInteraction.Hash(_hashFunc));
                }
            }

            return result;
        }

        private ISet<Interaction> CreateForBigMeeting(string eventId, Employee user, IEnumerable<Employee> participants, DateTime timestamp, int duration, RecurrenceType? recurrence)
        {
            var result = new HashSet<Interaction>(new InteractionEqualityComparer());

            var externalParticipants = participants.Where(x => x.IsExternal);

            foreach (var externalParticipant in externalParticipants)
            {
                var interaction = Interaction.CreateMeeting(
                    timestamp: timestamp,
                    source: user,
                    target: externalParticipant,
                    eventId: eventId,
                    recurring: recurrence,
                    duration: duration);

                result.Add(interaction.Hash(_hashFunc));
            }

            return result;
        }

        private static bool IsSmallMeeting(int participantsCount)
        {
            const int smallMeetingThreshold = 100;

            return participantsCount <= smallMeetingThreshold;
        }
    }
}