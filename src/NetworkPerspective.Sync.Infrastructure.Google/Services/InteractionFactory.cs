using System.Collections.Generic;

using Google.Apis.Calendar.v3.Data;
using Google.Apis.Gmail.v1.Data;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal class InteractionFactory
    {
        private readonly HashFunction _hashFunc;
        private readonly EmployeeCollection _employeeLookupTable;
        private readonly IClock _clock;
        private readonly ICombinationFactory _combinationFactory = new CombinationFactory();

        public InteractionFactory(HashFunction hash, EmployeeCollection employeeLookupTable, IClock clock)
        {
            _hashFunc = hash;
            _employeeLookupTable = employeeLookupTable;
            _clock = clock;
        }

        public ISet<Interaction> CreateFromEmail(Message email)
        {
            var sender = _employeeLookupTable.Find(email.GetSender());
            var timestamp = email.GetDateTime(_clock);

            var result = new HashSet<Interaction>(new InteractionEqualityComparer());

            foreach (var receiver in email.GetReceivers())
            {
                var interaction = Interaction.CreateEmail(
                    timestamp: timestamp,
                    source: sender,
                    target: _employeeLookupTable.Find(receiver),
                    eventId: email.Id);

                result.Add(interaction.Hash(_hashFunc));
            }

            return result;
        }

        public ISet<Interaction> CreateFromMeeting(Event meeting)
        {
            var recurrence = meeting.GetRecurrence();
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