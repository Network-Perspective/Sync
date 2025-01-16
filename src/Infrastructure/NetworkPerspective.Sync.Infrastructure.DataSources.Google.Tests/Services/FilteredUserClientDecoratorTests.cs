using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Criterias;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests.Services;

public class FilteredUserClientDecoratorTests
{
    private readonly ILogger<FilteredUserClientDecorator> _logger = NullLogger<FilteredUserClientDecorator>.Instance;
    private readonly Mock<IUsersClient> _usersClientMock = new();

    public FilteredUserClientDecoratorTests()
    {
        _usersClientMock.Reset();
    }

    [Fact]
    public async Task ShouldUseCriterias()
    {
        // Arrange
        const string user1Email = "user1@networkperspective.io";
        const string user2Email = "user2@networkperspective.io";

        var user1 = new User { PrimaryEmail = user1Email };
        var user2 = new User { PrimaryEmail = user2Email };

        var users = new[] { user1, user2 };

        _usersClientMock
            .Setup(x => x.GetUsersAsync(It.IsAny<ICredential>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var timeRange = new TimeRange(null, null);
        var syncContext = new SyncContext(Guid.NewGuid(), "test", ConnectorConfig.Empty, ImmutableDictionary<string, string>.Empty, "foo".ToSecureString(), timeRange);

        var contextAccessor = new SyncContextAccessor() { SyncContext = syncContext };

        var criterias = new[]
        {
            new NotLikedUsersCriteria(user2Email)
        };

        var filter = new FilteredUserClientDecorator(_usersClientMock.Object, contextAccessor, criterias, _logger);

        // Act
        var actualUsers = await filter.GetUsersAsync(Mock.Of<ICredential>());


        // Assert
        actualUsers.Should().BeEquivalentTo([user1]);
    }

    [Fact]
    public async Task ShouldEmailFilter()
    {
        // Arrange
        const string user1Email = "user1@networkperspective.io";
        const string user2Email = "user2@networkperspective.io";

        var user1 = new User { PrimaryEmail = user1Email };
        var user2 = new User { PrimaryEmail = user2Email };

        var users = new[] { user1, user2 };

        _usersClientMock
            .Setup(x => x.GetUsersAsync(It.IsAny<ICredential>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(users);

        var timeRange = new TimeRange(null, null);
        var employeeFilter = new EmployeeFilter(["*"], [user2Email]);
        var connectorConfig = new ConnectorConfig(employeeFilter, CustomAttributesConfig.Empty);
        var syncContext = new SyncContext(Guid.NewGuid(), "test", connectorConfig, ImmutableDictionary<string, string>.Empty, "foo".ToSecureString(), timeRange);

        var contextAccessor = new SyncContextAccessor() { SyncContext = syncContext };

        var filter = new FilteredUserClientDecorator(_usersClientMock.Object, contextAccessor, [], _logger);

        // Act
        var actualUsers = await filter.GetUsersAsync(Mock.Of<ICredential>());


        // Assert
        actualUsers.Should().BeEquivalentTo([user1]);
    }

    private class NotLikedUsersCriteria(string notLikedUserEmail) : ICriteria
    {
        public IList<User> MeetCriteria(IList<User> users)
            => users
                .Where(x => x.PrimaryEmail != notLikedUserEmail)
                .ToList();
    }
}