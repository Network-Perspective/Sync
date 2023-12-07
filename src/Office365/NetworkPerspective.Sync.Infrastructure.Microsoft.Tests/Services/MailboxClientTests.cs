using System;
using System.Linq;
using System.Security;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Mappers;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Services
{
    public class MailboxClientTests : IClassFixture<MicrosoftClientFixture>
    {
        private readonly MicrosoftClientFixture _microsoftClientFixture;
        private readonly ILogger<UsersClient> _usersClientlogger = NullLogger<UsersClient>.Instance;
        private readonly ILogger<MailboxClient> _mailboxClientlogger = NullLogger<MailboxClient>.Instance;

        public MailboxClientTests(MicrosoftClientFixture microsoftClientFixture)
        {
            _microsoftClientFixture = microsoftClientFixture;
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldCollectFilteredMails()
        {
            // Arrange
            var stream = new TestableInteractionStream();
            var usersClient = new UsersClient(_microsoftClientFixture.GraphServiceClient, _usersClientlogger);

            var timeRange = new TimeRange(new DateTime(2021, 12, 01), new DateTime(2021, 12, 02));
            var syncContext = new SyncContext(Guid.NewGuid(), NetworkConfig.Empty, new NetworkProperties(), new SecureString(), timeRange, Mock.Of<IStatusLogger>(), Mock.Of<IHashingService>());
            var users = await usersClient.GetUsersAsync(syncContext);
            var employees = EmployeesMapper.ToEmployees(users, EmailFilter.Empty);

            var interactionFactory = new EmailInteractionFactory(HashFunction.Empty, employees, NullLogger<EmailInteractionFactory>.Instance);
            var mailboxClient = new MailboxClient(_microsoftClientFixture.GraphServiceClient, Mock.Of<ITasksStatusesCache>(), _mailboxClientlogger);

            // Act
            await mailboxClient.SyncInteractionsAsync(syncContext, stream, users.Select(x => x.Mail), interactionFactory);

            stream.SentInteractions.Should().HaveCount(55);
        }
    }
}