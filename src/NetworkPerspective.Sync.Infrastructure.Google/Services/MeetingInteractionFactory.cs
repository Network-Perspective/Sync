using System.Collections.Generic;

using Google.Apis.Calendar.v3.Data;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Meetings;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal class MeetingInteractionFactory
    {
        private readonly HashFunction _hashFunc;
        private readonly EmployeeCollection _employeeLookupTable;
        private readonly ICombinationFactory _combinationFactory = new CombinationFactory();

        public MeetingInteractionFactory(HashFunction hash, EmployeeCollection employeeLookupTable)
        {
            _hashFunc = hash;
            _employeeLookupTable = employeeLookupTable;
        }

        public ISet<Interaction> Create(Event meeting, RecurrenceType? recurrence)
        {
            var meetingDuration = meeting.GetDurationInMinutes();
            var meetingStart = meeting.GetStart();
            var participants = meeting.GetParticipants();

            var result = new HashSet<Interaction>(new InteractionEqualityComparer());

            foreach (var combination in _combinationFactory.CreateCombinations(participants))
            {
                var interaction = Interaction.CreateMeeting(
                    timestamp: meetingStart,
                    source: _employeeLookupTable.Find(combination.Source),
                    target: _employeeLookupTable.Find(combination.Target),
                    eventId: meeting.Id,
                    recurring: recurrence,
                    duration: meetingDuration);

                result.Add(interaction.Hash(_hashFunc));
            }

            return result;
        }
    }
}