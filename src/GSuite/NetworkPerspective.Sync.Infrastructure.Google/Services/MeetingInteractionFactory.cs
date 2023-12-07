using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Google.Apis.Calendar.v3.Data;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Meetings;
using NetworkPerspective.Sync.Infrastructure.Google.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Google.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal class MeetingInteractionFactory
    {
        private readonly HashFunction.Delegate _hashFunc;
        private readonly EmployeeCollection _employees;
        private readonly ILogger<MeetingInteractionFactory> _logger;

        public MeetingInteractionFactory(HashFunction.Delegate hash, EmployeeCollection employees, ILogger<MeetingInteractionFactory> logger)
        {
            _hashFunc = hash;
            _employees = employees;
            _logger = logger;
        }

        public ISet<Interaction> CreateForUser(Event meeting, string userEmail, RecurrenceType? recurrence)
        {
            try
            {
                var user = _employees.Find(userEmail);
                var duration = meeting.GetDurationInMinutes();
                var timestamp = meeting.GetStart();
                var participants = meeting
                    .GetParticipants()                                      // Participants as email address 
                    .Select(_employees.Find)                                // Map to Employee
                    .Where(x => !Employee.EqualityComparer.Equals(x, user)) // Skip the user
                    .Distinct(Employee.EqualityComparer);                   // Remove duplicates

                if (IsSmallMeeting(participants.Count()))
                    return CreateForSmallMeeting(meeting.Id, user, participants, timestamp, duration, recurrence);
                else
                    return CreateForBigMeeting(meeting.Id, user, participants, timestamp, duration, recurrence);
            }
            catch (NotSupportedEmailFormatException ex)
            {
                _logger.LogWarning("Invalid email format");
                _logger.LogTrace(ex, "Invalid email format");
                return ImmutableHashSet<Interaction>.Empty;
            }
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