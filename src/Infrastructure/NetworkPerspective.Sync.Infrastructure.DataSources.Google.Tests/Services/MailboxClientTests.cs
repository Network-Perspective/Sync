using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Common.Tests.Extensions;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests.Fixtures;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Services;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests.Services;

public class MailboxClientTests(GoogleClientFixture googleClientFixture) : IClassFixture<GoogleClientFixture>
{
    [Fact]
    [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
    public async Task ShouldReturnNonEmptyEmailCollection()
    {
        // Arrange
        const string userEmail = "nptestuser12@worksmartona.com";

        var googleConfig = new GoogleConfig
        {
            ApplicationName = GoogleClientFixture.ApplicationName,
            MaxMessagesPerUserDaily = 1000,
            SyncOverlapInMinutes = 0
        };

        var clock = new Clock();
        var retryPolicyProvider = new RetryPolicyProvider(NullLogger<RetryPolicyProvider>.Instance);

        var mailboxClient = new MailboxClient(Mock.Of<IGlobalStatusCache>(), Options.Create(googleConfig), googleClientFixture.CredentialProvider, retryPolicyProvider, Mock.Of<IStatusLogger>(), NullLoggerFactory.Instance, clock);

        var employees = new List<Employee>()
            .Add(userEmail);
        var employeesCollection = new EmployeeCollection(employees, null);
        var interactionFactory = new EmailInteractionFactory((x) => $"{x}_hashed", employeesCollection, clock, NullLogger<EmailInteractionFactory>.Instance);
        var stream = new TestableInteractionStream();
        var timeRange = new TimeRange(new DateTime(2022, 11, 01), new DateTime(2022, 12, 31));
        var syncContext = new SyncContext(Guid.NewGuid(), string.Empty, ConnectorConfig.Empty, ImmutableDictionary<string, string>.Empty, new SecureString(), timeRange);

        // Act
        await mailboxClient.SyncInteractionsAsync(syncContext, stream, [userEmail], interactionFactory);

        // Assert

        var result1 = stream.SentInteractions.Where(x => x.Timestamp.Date == new DateTime(2022, 11, 20));
        result1.Single(x => x.Source.Id.PrimaryId == "maciej@networkperspective.io_hashed" && x.Target.Id.PrimaryId == $"{userEmail}_hashed");

        var result2 = stream.SentInteractions.Where(x => x.Timestamp.Date == new DateTime(2022, 12, 24));
        result2.Single(x => x.Source.Id.PrimaryId == $"{userEmail}_hashed" && x.Target.Id.PrimaryId == "john@worksmartona.com_hashed");
    }
}