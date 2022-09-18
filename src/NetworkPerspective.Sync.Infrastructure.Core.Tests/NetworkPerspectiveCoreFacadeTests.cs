using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Core.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Core.Mappers;
using NetworkPerspective.Sync.Infrastructure.Core.Tests.Toolkit;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Core.Tests
{
    public class NetworkPerspectiveCoreFacadeTests
    {
        private readonly Mock<ISyncHashedClient> _clientMock = new Mock<ISyncHashedClient>();
        private readonly ILogger<NetworkPerspectiveCoreFacade> _logger = NullLogger<NetworkPerspectiveCoreFacade>.Instance;

        public NetworkPerspectiveCoreFacadeTests()
        {
            _clientMock.Reset();
        }

        public class ValidateToken : NetworkPerspectiveCoreFacadeTests
        {
            [Fact]
            public async Task ShouldThrowInvalidAccessTokenException()
            {
                // Arrange
                const string token = "token";

                _clientMock
                    .Setup(x => x.QueryAsync(token, CancellationToken.None))
                    .ThrowsAsync(new ApiException("message", StatusCodes.Status403Forbidden, "response", null, null));

                var facade = new NetworkPerspectiveCoreFacade(_clientMock.Object, CreateNpCoreOptions(), _logger);

                // Act
                Func<Task<TokenValidationResponse>> func = async () => await facade.ValidateTokenAsync(new NetworkCredential(string.Empty, token).SecurePassword);

                // Assert
                await func.Should().ThrowExactlyAsync<InvalidTokenException>();
            }
        }

        public class PushInteractions : NetworkPerspectiveCoreFacadeTests
        {
            [Theory]
            [InlineData(100)]
            public async Task ShouldSendAllData(int count)
            {
                // Arrange
                var interactions = InteractionsFactory.Create(count);

                var sentInteractions = new List<HashedInteraction>();

                _clientMock
                    .Setup(x => x.SyncInteractionsAsync(It.IsAny<SyncHashedInteractionsCommand>(), It.IsAny<CancellationToken>()))
                    .Callback<SyncHashedInteractionsCommand, CancellationToken>((x, y) => sentInteractions.AddRange(x.Interactions));

                var dataSourceIdName = "SlackId";
                var options = CreateNpCoreOptions(dataSourceIdName: dataSourceIdName);
                var facade = new NetworkPerspectiveCoreFacade(_clientMock.Object, options, _logger);

                // Act
                await facade.PushInteractionsAsync("foo".ToSecureString(), interactions);

                // Assert
                var expectedSentInteractions = interactions.Select(x => InteractionMapper.DomainIntractionToDto(x, dataSourceIdName));
                sentInteractions.Should().BeEquivalentTo(expectedSentInteractions);
            }

            [Theory]
            [InlineData(0)]
            [InlineData(99)]
            [InlineData(100)]
            [InlineData(101)]
            public async Task ShouldPartitionData(int count)
            {
                // Arrange
                var interactions = InteractionsFactory.Create(count);
                var maxInteractionsPerRequestCount = 10;

                var options = CreateNpCoreOptions(maxInteractionsPerRequestCount: maxInteractionsPerRequestCount);
                var facade = new NetworkPerspectiveCoreFacade(_clientMock.Object, options, _logger);

                // Act
                await facade.PushInteractionsAsync(new NetworkCredential(string.Empty, "foo").SecurePassword, interactions);

                // Assert
                var expectedCallsCount = (int)Math.Ceiling(count / (double)maxInteractionsPerRequestCount);
                _clientMock.Verify(x => x.SyncInteractionsAsync(It.IsAny<SyncHashedInteractionsCommand>(), It.IsAny<CancellationToken>()), Times.Exactly(expectedCallsCount));
            }
        }

        public class PushGroups : NetworkPerspectiveCoreFacadeTests
        {
            [Fact]
            public async Task ShouldSkipGroupsWithNullOrEmptyName()
            {
                // Arrange
                var group1 = Group.Create("id1", "name1", "category1");
                var group2 = Group.Create("id2", string.Empty, "category2");
                var group3 = Group.Create("id3", null, "category3");

                var groups = new[]
                {
                    group1, group2, group3
                };

                var sentGroups = new List<HashedGroup>();

                _clientMock
                    .Setup(x => x.SyncGroupsAsync(It.IsAny<SyncHashedGroupStructureCommand>(), It.IsAny<CancellationToken>()))
                    .Callback<SyncHashedGroupStructureCommand, CancellationToken>((x, y) => sentGroups.AddRange(x.Groups));

                var options = CreateNpCoreOptions();
                var facade = new NetworkPerspectiveCoreFacade(_clientMock.Object, options, _logger);

                // Act
                await facade.PushGroupsAsync(new NetworkCredential(string.Empty, "foo").SecurePassword, groups);

                // Assert
                var expectedSentGroups = new[] { group1 }.Select(GroupsMapper.ToGroup);
                sentGroups.Should().BeEquivalentTo(expectedSentGroups);
            }
        }

        private static IOptions<NetworkPerspectiveCoreConfig> CreateNpCoreOptions(string url = "foo", int maxInteractionsPerRequestCount = 10, string dataSourceIdName = "bar")
            => Options.Create(new NetworkPerspectiveCoreConfig
            {
                BaseUrl = url,
                MaxInteractionsPerRequestCount = maxInteractionsPerRequestCount,
                DataSourceIdName = dataSourceIdName
            });
    }
}