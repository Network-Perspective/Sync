using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Fixtures;

using Xunit;
using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Domain.Networks;
using System.Security;
using Moq;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Services
{
    public class ChannelsClientTests : IClassFixture<MicrosoftClientWithTeamsFixture>
    {
        private readonly MicrosoftClientWithTeamsFixture _microsoftClientFixture;
        private readonly ILogger<UsersClient> _usersClientlogger = NullLogger<UsersClient>.Instance;
        private readonly ILogger<ChannelsClient> _channelsClientLogger = NullLogger<ChannelsClient>.Instance;

        public ChannelsClientTests(MicrosoftClientWithTeamsFixture microsoftClientFixture)
        {
            _microsoftClientFixture = microsoftClientFixture;
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldSyncInteractions()
        {
            // Arrange
            var stream = new TestableInteractionStream();
            var usersClient = new UsersClient(_microsoftClientFixture.Client, _usersClientlogger);

            var timeRange = new TimeRange(new DateTime(2023, 01, 10), new DateTime(2023, 12, 11));
            var syncContext = new SyncContext(Guid.NewGuid(), NetworkConfig.Empty, new NetworkProperties(), new SecureString(), timeRange, Mock.Of<IStatusLogger>(), Mock.Of<IHashingService>());
            var users = await usersClient.GetUsersAsync(syncContext);
            var employees = EmployeesMapper.ToEmployees(users, EmailFilter.Empty);

            var channelsClient = new ChannelsClient(_microsoftClientFixture.Client, Mock.Of<ITasksStatusesCache>(), _channelsClientLogger);

            // Act
            await channelsClient.SyncInteractionsAsync(syncContext, stream, users.Select(x => x.Mail));
        }
    }
}
