using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Common.Tests.Extensions;
using NetworkPerspective.Sync.Infrastructure.Google.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Google.Tests.Services
{
    public class MailboxClientTests : IClassFixture<GoogleClientFixture>
    {
        private readonly GoogleClientFixture _googleClientFixture;

        public MailboxClientTests(GoogleClientFixture googleClientFixture)
        {
            _googleClientFixture = googleClientFixture;
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldReturnNonEmptyEmailCollection()
        {
            // Arrange
            const string userEmail = "nptestuser12@worksmartona.com";

            var googleConfig = new GoogleConfig
            {
                ApplicationName = "gmail_app",
                MaxMessagesPerUserDaily = 1000,
                SyncOverlapInMinutes = 0
            };

            var clock = new Clock();

            var mailboxClient = new MailboxClient(Mock.Of<IStatusLogger>(), Mock.Of<ITasksStatusesCache>(), Options.Create(googleConfig), NullLoggerFactory.Instance, clock);

            var employees = new List<Employee>()
                .Add(userEmail);
            var employeesCollection = new EmployeeCollection(employees, null);
            var interactionFactory = new EmailInteractionFactory((x) => $"{x}_hashed", employeesCollection, clock);
            var stream = new TestableInteractionStream();

            // Act
            await mailboxClient.SyncInteractionsAsync(stream, Guid.NewGuid(), new[] { Employee.CreateInternal(EmployeeId.Create(userEmail, userEmail), Array.Empty<Group>()) }, new DateTime(2021, 11, 01), _googleClientFixture.Credential, interactionFactory);

            // Assert

            var result1 = stream.SentInteractions.Where(x => x.Timestamp.Date == new DateTime(2022, 11, 20));
            result1.Single(x => x.Source.Id.PrimaryId == "maciej@networkperspective.io_hashed" && x.Target.Id.PrimaryId == $"{userEmail}_hashed");

            var result2 = stream.SentInteractions.Where(x => x.Timestamp.Date == new DateTime(2022, 12, 24));
            result2.Single(x => x.Source.Id.PrimaryId == $"{userEmail}_hashed" && x.Target.Id.PrimaryId == "john@worksmartona.com_hashed");

        }
    }
}