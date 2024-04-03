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
    internal interface IEmailInteractionFactory
    {
        ISet<Interaction> CreateForUser(Message message, string userEmail);
    }

    internal class EmailInteractionFactory : IEmailInteractionFactory
    {
        private readonly HashFunction.Delegate _hash;
        private readonly EmployeeCollection _employees;
        private readonly ILogger<EmailInteractionFactory> _logger;

        public EmailInteractionFactory(HashFunction.Delegate hash, EmployeeCollection employees, ILogger<EmailInteractionFactory> logger)
        {
            _hash = hash;
            _employees = employees;
            _logger = logger;
        }

        public ISet<Interaction> CreateForUser(Message message, string userEmail)
        {
            try
            {
                var user = _employees.Find(userEmail);
                var sender = _employees.Find(message.Sender?.EmailAddress?.Address);
                var recipients = message.ToRecipients
                    .Union(message.CcRecipients)
                    .Union(message.BccRecipients)
                    .Where(x => x.EmailAddress?.Address is not null)
                    .Select(x => _employees.Find(x.EmailAddress?.Address))
                    .Distinct(Employee.EqualityComparer);

                if (user.IsExternal)
                    return ImmutableHashSet<Interaction>.Empty;

                if (message.SentDateTime is null || !message.SentDateTime.HasValue)
                    return ImmutableHashSet<Interaction>.Empty;

                var timestamp = message.SentDateTime.Value.UtcDateTime;

                if (IsOutgoing(user, sender))
                    return CreateForOutgoing(message.Id, user, recipients, timestamp);
                else
                    return CreateForIncoming(message.Id, sender, user, timestamp);
            }
            catch (Exception)
            {
                _logger.LogDebug("Exception has been thrown in {Class}", typeof(EmailInteractionFactory));
                _logger.LogDebug("Message: {Item}", message);
                _logger.LogDebug("Message.SentDateTime: {Item}", message.SentDateTime);
                _logger.LogDebug("Message.SentDateTime.Value: {Item}", message.SentDateTime.Value);
                _logger.LogDebug("Message.SentDateTime.Value.UtcDateTime: {Item}", message.SentDateTime.Value.UtcDateTime);
                _logger.LogDebug("Message.Id: {Item}", message.Id);
                throw;
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