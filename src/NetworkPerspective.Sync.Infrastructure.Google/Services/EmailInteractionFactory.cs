using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Google.Apis.Gmail.v1.Data;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Google.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal class EmailInteractionFactory
    {
        private readonly HashFunction.Delegate _hashFunc;
        private readonly EmployeeCollection _employeeLookupTable;
        private readonly IClock _clock;
        private readonly ILogger<EmailInteractionFactory> _logger;

        public EmailInteractionFactory(HashFunction.Delegate hash, EmployeeCollection employeeLookupTable, IClock clock, ILogger<EmailInteractionFactory> logger)
        {
            _hashFunc = hash;
            _employeeLookupTable = employeeLookupTable;
            _clock = clock;
            _logger = logger;
        }

        public ISet<Interaction> CreateForUser(Message message, string userEmail)
        {
            try
            {
                var user = _employeeLookupTable.Find(userEmail);
                var sender = _employeeLookupTable.Find(message.GetSender());
                var recipients = message
                    .GetRecipients()
                    .Select(_employeeLookupTable.Find)
                    .Distinct(Employee.EqualityComparer);
                var timestamp = message.GetDateTime(_clock);

                if (user.IsExternal)
                    return ImmutableHashSet<Interaction>.Empty;

                if (IsOutgoing(user, sender))
                    return CreateForOutgoing(message.Id, user, recipients, timestamp);
                else
                    return CreateForIncoming(message.Id, sender, user, timestamp);
            }
            catch (NotSupportedEmailFormatException ex)
            {
                _logger.LogWarning("Invalid email format");
                _logger.LogTrace(ex, "Invalid email format");
                return ImmutableHashSet<Interaction>.Empty;
            }
        }

        private ISet<Interaction> CreateForOutgoing(string eventId, Employee user, IEnumerable<Employee> recipients, DateTime timestamp)
        {
            var result = new HashSet<Interaction>();

            foreach (var recipient in recipients)
            {
                var interaction = Interaction.CreateEmail(
                    timestamp: timestamp,
                    source: user,
                    target: recipient,
                    eventId: eventId);

                result.Add(interaction.Hash(_hashFunc));
            }

            return result;
        }

        private ISet<Interaction> CreateForIncoming(string eventId, Employee sender, Employee user, DateTime timestamp)
        {
            if (!sender.IsExternal)
                return ImmutableHashSet<Interaction>.Empty;

            var interaction = Interaction.CreateEmail(
                timestamp: timestamp,
                source: sender,
                target: user,
                eventId: eventId);

            return new HashSet<Interaction> { interaction.Hash(_hashFunc) };
        }

        private static bool IsOutgoing(Employee user, Employee sender)
            => Employee.EqualityComparer.Equals(user, sender);
    }
}