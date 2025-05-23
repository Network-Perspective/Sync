﻿using System;
using System.Collections.Immutable;
using System.Linq;
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
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Services;

public class ChatsClientTests(MicrosoftClientWithTeamsFixture microsoftClientFixture) : IClassFixture<MicrosoftClientWithTeamsFixture>
{
    private readonly ILogger<UsersClient> _usersClientLogger = NullLogger<UsersClient>.Instance;
    private readonly ILogger<ChatsClient> _chatsClientLogger = NullLogger<ChatsClient>.Instance;

    [Fact]
    [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
    public async Task ShouldSyncInteractions()
    {
        // Arrange
        var stream = new TestableInteractionStream();
        var usersClient = new UsersClient(microsoftClientFixture.Client, Mock.Of<IGlobalStatusCache>(), _usersClientLogger);

        var timeRange = new TimeRange(new DateTime(2021, 01, 01), new DateTime(2024, 12, 11));
        var syncContext = new SyncContext(Guid.NewGuid(), string.Empty, ConnectorConfig.Empty, ImmutableDictionary<string, string>.Empty, new SecureString(), timeRange);
        var users = await usersClient.GetUsersAsync(syncContext);
        var employees = EmployeesMapper.ToEmployees(users, x => $"{x}_hashed", EmployeeFilter.Empty, true);
        var interactionsFactory = new ChatInteractionFactory(x => $"{x}_hashed", employees);

        var chatsClient = new ChatsClient(microsoftClientFixture.Client, Mock.Of<IGlobalStatusCache>(), Mock.Of<IStatusLogger>(), _chatsClientLogger);
        var emails = employees.GetAllInternal().Select(x => x.Id.PrimaryId);

        // Act
        var result = await chatsClient.SyncInteractionsAsync(syncContext, stream, emails, interactionsFactory);
    }
}