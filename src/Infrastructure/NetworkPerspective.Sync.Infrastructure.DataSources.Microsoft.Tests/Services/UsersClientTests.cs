﻿using System;
using System.Collections.Immutable;
using System.Security;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Fixtures;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Services;

public class UsersClientTests(MicrosoftClientBasicFixture microsoftClientFixture) : IClassFixture<MicrosoftClientBasicFixture>
{
    private readonly ILogger<UsersClient> _logger = NullLogger<UsersClient>.Instance;

    [Fact]
    [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
    public async Task ShouldBeAbleToGetUsers()
    {
        // Arrange
        var client = new UsersClient(microsoftClientFixture.Client, Mock.Of<IGlobalStatusCache>(), _logger);
        var timeRange = new TimeRange(new DateTime(2022, 12, 21), new DateTime(2022, 12, 22));
        var filter = new EmployeeFilter(["group:Sample Team Site"], Array.Empty<string>());
        var networkConfig = new ConnectorConfig(filter, CustomAttributesConfig.Empty);
        var syncContext = new SyncContext(Guid.NewGuid(), string.Empty, networkConfig, ImmutableDictionary<string, string>.Empty, new SecureString(), timeRange);

        // Act
        var result = await client.GetUsersAsync(syncContext);

        result.Should().NotBeEmpty();
    }
}