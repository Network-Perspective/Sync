using System.Collections.Generic;
using System.Linq;

using FluentAssertions;

using Google.Apis.Gmail.v1.Data;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests.Extensions;
using NetworkPerspective.Sync.Infrastructure.Google.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Google.Tests.Services
{
    public class EmailInteractionFactoryTests
    {
        private readonly ILogger<EmailInteractionFactory> _logger = NullLogger<EmailInteractionFactory>.Instance;

        [Fact]
        public void ShouldCreateForAllRecipientsOnOutgoing()
        {
            // Arrange
            const string user1Email = "user1@networkperspective.io";
            const string user2Email = "user2 <user2@networkperspective.io>";
            const string user2Email_alias = "user2_alias@networkperspective.io";
            const string user3Email = "user3@networkperspective.io";
            const string user4Email = "user4@networkperspective.io";
            const string user5Email = "user5@networkperspective.io";
            const string externalUserEmail = "external@foo.com";

            var user2Id = EmployeeId.CreateWithAliases("user2@networkperspective.io", "test", new[] { user2Email_alias }, EmailFilter.Empty);
            var user2 = Employee.CreateInternal(user2Id, Enumerable.Empty<Group>());

            var email = new Message()
            {
                Payload = new MessagePart
                {
                    Headers = new[]
                    {
                        new MessagePartHeader { Name = "from", Value = user1Email},
                        new MessagePartHeader { Name = "to", Value = string.Join(", ", user2Email, user2Email_alias, user3Email)},
                        new MessagePartHeader { Name = "cc", Value = string.Join(", ", user4Email, externalUserEmail)},
                        new MessagePartHeader { Name = "bcc", Value = user5Email},
                    }
                }
            };

            var employees = new List<Employee> { user2 }
                .Add(user1Email)
                .Add(user3Email)
                .Add(user4Email)
                .Add(user5Email);

            var employeesCollection = new EmployeeCollection(employees, null);

            // Act
            var interactions = new EmailInteractionFactory(x => $"{x}_hashed", employeesCollection, new Clock(), _logger)
                .CreateForUser(email, user1Email);

            // Assert
            var emailInteractions = interactions.Where(x => x.Type == InteractionType.Email);
            emailInteractions.Should().HaveCount(5);
            emailInteractions.Where(x => x.Source.Id.PrimaryId == $"{user1Email}_hashed" && x.Target.Id.PrimaryId == "user2@networkperspective.io_hashed").Should().ContainSingle();
            emailInteractions.Where(x => x.Source.Id.PrimaryId == $"{user1Email}_hashed" && x.Target.Id.PrimaryId == $"{user3Email}_hashed").Should().ContainSingle();
            emailInteractions.Where(x => x.Source.Id.PrimaryId == $"{user1Email}_hashed" && x.Target.Id.PrimaryId == $"{user4Email}_hashed").Should().ContainSingle();
            emailInteractions.Where(x => x.Source.Id.PrimaryId == $"{user1Email}_hashed" && x.Target.Id.PrimaryId == $"{user5Email}_hashed").Should().ContainSingle();
            emailInteractions.Where(x => x.Source.Id.PrimaryId == $"{user1Email}_hashed" && x.Target.Id.PrimaryId == $"{externalUserEmail}_hashed").Should().ContainSingle();
        }

        [Fact]
        public void ShouldCreateOnlyForExternalOnIncoming()
        {
            // Arrange
            const string user1Email = "user1@networkperspective.io";
            const string user2Email = "user2 <user2@networkperspective.io>";
            const string externalUserEmail = "external@foo.com";

            var email = new Message()
            {
                Payload = new MessagePart
                {
                    Headers = new[]
                    {
                        new MessagePartHeader { Name = "from", Value = externalUserEmail},
                        new MessagePartHeader { Name = "to", Value = user2Email},
                        new MessagePartHeader { Name = "cc", Value = user1Email},
                    }
                }
            };

            var employees = new List<Employee>()
                .Add(user1Email)
                .Add("user2@networkperspective.io");

            var employeesCollection = new EmployeeCollection(employees, null);

            // Act
            var interactions = new EmailInteractionFactory(x => $"{x}_hashed", employeesCollection, new Clock(), _logger)
                .CreateForUser(email, user1Email);

            // Assert
            var emailInteractions = interactions.Where(x => x.Type == InteractionType.Email);
            emailInteractions.Should().HaveCount(1);
            emailInteractions.Where(x => x.Source.Id.PrimaryId == $"{externalUserEmail}_hashed" && x.Target.Id.PrimaryId == "user1@networkperspective.io_hashed").Should().ContainSingle();
        }
    }
}