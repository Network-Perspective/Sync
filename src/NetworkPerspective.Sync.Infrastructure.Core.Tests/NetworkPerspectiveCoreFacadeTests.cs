using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using HandlebarsDotNet;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Networks;
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
                await facade.PushGroupsAsync("foo".ToSecureString(), groups);

                // Assert
                var expectedSentGroups = new[] { group1 }.Select(GroupsMapper.ToGroup);
                sentGroups.Should().BeEquivalentTo(expectedSentGroups);
            }
        }

        public class GetNetworkConfig : NetworkPerspectiveCoreFacadeTests
        {
            [Fact]
            public async Task ShouldBeAbleToHandleNullReponses()
            {
                // Arrange
                var responseFromApiDto = new HashedDataSourceSettingsResult
                {
                    Blacklist = null,
                    Whitelist = null,
                    CustomAttributes = null
                };

                _clientMock.Setup(x => x.SettingsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(responseFromApiDto);

                var options = CreateNpCoreOptions();
                var facade = new NetworkPerspectiveCoreFacade(_clientMock.Object, options, _logger);

                // Act
                var result = await facade.GetNetworkConfigAsync("foo".ToSecureString());

                // Assert
                var expectedResult = new NetworkConfig(EmailFilter.Empty, CustomAttributesConfig.Empty);

            }
        }

        public class ReportSyncStartAsync : NetworkPerspectiveCoreFacadeTests
        {
            [Fact]
            public async Task ShouldCallEndpoint()
            {
                // Arrange
                var start = new DateTime(2022, 01, 01);
                var end = new DateTime(2022, 01, 02);
                var timeRange = new TimeRange(start, end);
                var options = CreateNpCoreOptions();
                var facade = new NetworkPerspectiveCoreFacade(_clientMock.Object, options, _logger);

                // Act
                await facade.ReportSyncStartAsync("foo".ToSecureString(), timeRange);

                // Assert
                _clientMock.Verify(x => x.ReportStartAsync(
                        It.Is<ReportSyncStartedCommand>(x => x.SyncPeriodStart == start && x.SyncPeriodEnd == end),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            }

            [Fact]
            public async Task ShouldThrowSpecificException()
            {
                // Arrange
                var start = new DateTime(2022, 01, 01);
                var end = new DateTime(2022, 01, 02);
                var timeRange = new TimeRange(start, end);
                var options = CreateNpCoreOptions();
                var facade = new NetworkPerspectiveCoreFacade(_clientMock.Object, options, _logger);

                _clientMock
                    .Setup(x => x.ReportStartAsync(It.IsAny<ReportSyncStartedCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception());

                // Act
                Func<Task> func = async () => await facade.ReportSyncStartAsync("foo".ToSecureString(), timeRange);

                // Assert
                await func.Should().ThrowAsync<NetworkPerspectiveCoreException>();
            }
        }

        public class ReportSyncSuccessfulAsync : NetworkPerspectiveCoreFacadeTests
        {
            [Fact]
            public async Task ShouldCallEndpoint()
            {
                // Arrange
                var start = new DateTime(2022, 01, 01);
                var end = new DateTime(2022, 01, 02);
                var timeRange = new TimeRange(start, end);
                var options = CreateNpCoreOptions();
                var facade = new NetworkPerspectiveCoreFacade(_clientMock.Object, options, _logger);

                // Act
                await facade.ReportSyncSuccessfulAsync("foo".ToSecureString(), timeRange);

                // Assert
                _clientMock.Verify(x => x.ReportCompletedAsync(
                        It.Is<ReportSyncCompletedCommand>(x => x.SyncPeriodStart == start && x.SyncPeriodEnd == end && x.Success == true),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            }

            [Fact]
            public async Task ShouldNotThrowException()
            {
                // Arrange
                var start = new DateTime(2022, 01, 01);
                var end = new DateTime(2022, 01, 02);
                var timeRange = new TimeRange(start, end);
                var options = CreateNpCoreOptions();
                var facade = new NetworkPerspectiveCoreFacade(_clientMock.Object, options, _logger);

                _clientMock
                    .Setup(x => x.ReportCompletedAsync(It.IsAny<ReportSyncCompletedCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception());

                // Act
                Func<Task> func = async () => await facade.ReportSyncSuccessfulAsync("foo".ToSecureString(), timeRange);

                // Assert
                await func.Should().ThrowAsync<NetworkPerspectiveCoreException>();
            }
        }

        public class ReportSyncFailedAsync : NetworkPerspectiveCoreFacadeTests
        {
            [Fact]
            public async Task ShouldCallEndpoint()
            {
                // Arrange
                var start = new DateTime(2022, 01, 01);
                var end = new DateTime(2022, 01, 02);
                var message = "message";
                var timeRange = new TimeRange(start, end);
                var options = CreateNpCoreOptions();
                var facade = new NetworkPerspectiveCoreFacade(_clientMock.Object, options, _logger);

                // Act
                await facade.TryReportSyncFailedAsync("foo".ToSecureString(), timeRange, message);

                // Assert
                _clientMock.Verify(x => x.ReportCompletedAsync(
                        It.Is<ReportSyncCompletedCommand>(x => x.SyncPeriodStart == start && x.SyncPeriodEnd == end && x.Success == false && x.Message == message),
                        It.IsAny<CancellationToken>()),
                    Times.Once);
            }

            [Fact]
            public async Task ShouldNotThrowException()
            {
                // Arrange
                var start = new DateTime(2022, 01, 01);
                var end = new DateTime(2022, 01, 02);
                var timeRange = new TimeRange(start, end);
                var options = CreateNpCoreOptions();
                var facade = new NetworkPerspectiveCoreFacade(_clientMock.Object, options, _logger);

                _clientMock
                    .Setup(x => x.ReportCompletedAsync(It.IsAny<ReportSyncCompletedCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception());

                // Act
                Func<Task> func = async () => await facade.TryReportSyncFailedAsync("foo".ToSecureString(), timeRange, "bar");

                // Assert
                await func.Should().NotThrowAsync<Exception>();
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