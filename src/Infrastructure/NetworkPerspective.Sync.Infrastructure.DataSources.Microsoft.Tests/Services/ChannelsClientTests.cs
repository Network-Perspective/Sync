﻿using System;
using System.Security;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Mappers;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Fixtures;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Services
{
    public class ChannelsClientTests : IClassFixture<MicrosoftClientWithTeamsFixture>
    {
        private readonly MicrosoftClientWithTeamsFixture _microsoftClientFixture;
        private readonly ILogger<UsersClient> _usersClientLogger = NullLogger<UsersClient>.Instance;
        private readonly ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

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
            var usersClient = new UsersClient(_microsoftClientFixture.Client, _usersClientLogger);

            var timeRange = new TimeRange(new DateTime(2023, 01, 10), new DateTime(2023, 12, 11));
            var syncContext = new SyncContext(Guid.NewGuid(), string.Empty, ConnectorConfig.Empty, [], new SecureString(), timeRange, Mock.Of<IHashingService>());
            var users = await usersClient.GetUsersAsync(syncContext);
            var employees = EmployeesMapper.ToEmployees(users, x => $"{x}_hashed", EmployeeFilter.Empty, true);
            var interactionsFactory = new ChannelInteractionFactory(x => $"{x}_hashed", employees);

            var channelsClient = new ChannelsClient(_microsoftClientFixture.Client, Mock.Of<ITasksStatusesCache>(), _loggerFactory);

            // Act
            var channels = await channelsClient.GetAllChannelsAsync();
            await channelsClient.SyncInteractionsAsync(syncContext, channels, stream, interactionsFactory);
        }
    }
}