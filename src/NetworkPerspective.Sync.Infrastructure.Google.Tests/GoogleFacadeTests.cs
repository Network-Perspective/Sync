using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Google.Tests
{
    public class GoogleFacadeTests
    {
        private readonly Mock<INetworkService> _networkServiceMock = new Mock<INetworkService>();
        private readonly Mock<ISecretRepository> _secretRepositoryMock = new Mock<ISecretRepository>();
        private readonly Mock<ICredentialsProvider> _credentialsProviderMock = new Mock<ICredentialsProvider>();
        private readonly Mock<IMailboxClient> _mailboxClientMock = new Mock<IMailboxClient>();
        private readonly Mock<ICalendarClient> _calendarClientMock = new Mock<ICalendarClient>();
        private readonly Mock<IUsersClient> _usersClientMock = new Mock<IUsersClient>();
        private readonly Mock<IHashingServiceFactory> _hashingServiceFactoryMock = new Mock<IHashingServiceFactory>();
        private readonly Mock<IHashingService> _hashingServiceMock = new Mock<IHashingService>();
        private readonly Mock<IClock> _clockMock = new Mock<IClock>();
        private readonly ILogger<GoogleFacade> _logger = NullLogger<GoogleFacade>.Instance;

        public GoogleFacadeTests()
        {
            _networkServiceMock.Reset();
            _secretRepositoryMock.Reset();
            _credentialsProviderMock.Reset();
            _mailboxClientMock.Reset();
            _calendarClientMock.Reset();
            _usersClientMock.Reset();
            _hashingServiceFactoryMock.Reset();
            _hashingServiceMock.Reset();
            _clockMock.Reset();

            _hashingServiceFactoryMock
                .Setup(x => x.CreateAsync(It.IsAny<ISecretRepository>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_hashingServiceMock.Object);
        }

        public class GetInteractions : GoogleFacadeTests
        {
            [Fact]
            public async Task ShouldNotSetEmployeesInContext()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var options = CreateOptions();

                _mailboxClientMock
                    .Setup(x => x.GetInteractionsAsync(networkId, It.IsAny<IEnumerable<Employee>>(), It.IsAny<DateTime>(), It.IsAny<GoogleCredential>(), It.IsAny<InteractionFactory>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new HashSet<Interaction>());

                _calendarClientMock
                    .Setup(x => x.GetInteractionsAsync(networkId, It.IsAny<IEnumerable<Employee>>(), It.IsAny<TimeRange>(), It.IsAny<GoogleCredential>(), It.IsAny<InteractionFactory>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new HashSet<Interaction>());

                var facade = new GoogleFacade(
                    _networkServiceMock.Object,
                    _secretRepositoryMock.Object,
                    _credentialsProviderMock.Object,
                    _mailboxClientMock.Object,
                    _calendarClientMock.Object,
                    _usersClientMock.Object,
                    _hashingServiceFactoryMock.Object,
                    _clockMock.Object,
                    options, _logger);

                var syncContext = new SyncContext(networkId, NetworkConfig.Empty, "foo".ToSecureString(), DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);

                // Act
                await facade.GetInteractions(syncContext);

                // Assert
                syncContext.Contains<EmployeeCollection>().Should().BeFalse();
                syncContext.Contains<IEnumerable<User>>().Should().BeFalse();
            }
        }

        public class GetEmployees : GoogleFacadeTests
        {
            [Fact]
            public async Task ShouldNotSetEmployeesInContext()
            {
                // Arrange
                var networkId = Guid.NewGuid();
                var options = CreateOptions();

                var facade = new GoogleFacade(
                    _networkServiceMock.Object,
                    _secretRepositoryMock.Object,
                    _credentialsProviderMock.Object,
                    _mailboxClientMock.Object,
                    _calendarClientMock.Object,
                    _usersClientMock.Object,
                    _hashingServiceFactoryMock.Object,
                    _clockMock.Object,
                    options, _logger);

                var syncContext = new SyncContext(networkId, NetworkConfig.Empty, "foo".ToSecureString(), DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);

                // Act
                await facade.GetEmployees(syncContext);

                // Assert
                syncContext.Contains<EmployeeCollection>().Should().BeFalse();
                syncContext.Contains<IEnumerable<User>>().Should().BeFalse();
            }
        }

        private IOptions<GoogleConfig> CreateOptions()
        {
            return Options.Create(new GoogleConfig());
        }
    }
}