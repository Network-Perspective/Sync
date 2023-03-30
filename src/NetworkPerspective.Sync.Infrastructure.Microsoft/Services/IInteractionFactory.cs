using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Services
{
    public interface IInteractionFactory
    {
        ISet<Interaction> CreateForUser(Message message, string userEmail);
    }

    internal class InteractionFactory : IInteractionFactory
    {
        private readonly HashFunction.Delegate _hash;
        private readonly EmployeeCollection _employees;
        private readonly ILogger<InteractionFactory> _logger;

        public InteractionFactory(HashFunction.Delegate hash, EmployeeCollection employees, ILogger<InteractionFactory> logger)
        {
            _hash = hash;
            _employees = employees;
            _logger = logger;
        }

        public ISet<Interaction> CreateForUser(Message message, string userEmail)
        {
            var user = _employees.Find(userEmail);
            var sender = _employees.Find(message.Sender?.EmailAddress?.Address);
            var recipients = message.ToRecipients
                .Union(message.CcRecipients)
                .Union(message.BccRecipients)
                .Select(x => _employees.Find(x.EmailAddress?.Address))
                .Distinct(Employee.EqualityComparer);
            var timestamp = message.SentDateTime.Value.UtcDateTime;

            if (user.IsExternal)
                return ImmutableHashSet<Interaction>.Empty;

            if (IsOutgoing(user, sender))
                return CreateForOutgoing(message.Id, user, recipients, timestamp);
            else
                return CreateForIncoming(message.Id, sender, user, timestamp);
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

                result.Add(interaction.Hash(_hash));
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

            return new HashSet<Interaction> { interaction.Hash(_hash) };
        }

        private static bool IsOutgoing(Employee user, Employee sender)
            => Employee.EqualityComparer.Equals(user, sender);
    }
}