﻿using System;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.AspNetCore.Http;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Core.Exceptions;
using NetworkPerspective.Sync.Common.Tests.Fixtures;
using NetworkPerspective.Sync.GSuite.Client;
using NetworkPerspective.Sync.GSuite.Tests.Fixtures;
using NetworkPerspective.Sync.Infrastructure.Google;

using Xunit;

namespace NetworkPerspective.Sync.GSuite.Tests
{
    [Collection(GSuiteTestsCollection.Name)]
    public class BaseTest
    {
        private readonly InMemoryHostedServiceFixture<Startup> _service;

        public BaseTest(InMemoryHostedServiceFixture<Startup> service)
        {
            _service = service;
            service.Reset();
        }

        [Fact]
        public async Task ShouldSetupNetworkProperly()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var httpClient = _service.CreateDefaultClient();

            const string adminEmail = "admin@networkperspective.io";

            _service.NetworkPerspectiveCoreMock
                .Setup(x => x.ValidateTokenAsync(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenValidationResponse(networkId, Guid.NewGuid()));

            // Act
            var result = await new NetworksClient(httpClient)
                .NetworksPostAsync(adminEmail, null);

            // Assert
            _service.SecretRepositoryMock.Verify(x => x.SetSecretAsync($"np-token-GSuite-{networkId}", It.Is<SecureString>(x => x.ToSystemString() == _service.ValidToken), It.IsAny<CancellationToken>()), Times.Once);

            using var unitOfWork = _service.UnitOfWorkFactory.Create();
            var networksRepository = unitOfWork.GetNetworkRepository<GoogleNetworkProperties>();
            var network = await networksRepository.FindAsync(networkId);
            network.Properties.AdminEmail.Should().Be(adminEmail);
            network.Properties.ExternalKeyVaultUri.Should().BeNull();
        }

        [Fact]
        public async Task ShouldSetupSchedulesProperly()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var httpClient = _service.CreateDefaultClient();

            const string adminEmail = "admin@networkperspective.io";

            _service.NetworkPerspectiveCoreMock
                .Setup(x => x.ValidateTokenAsync(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenValidationResponse(networkId, Guid.NewGuid()));

            await new NetworksClient(httpClient)
                .NetworksPostAsync(adminEmail, null);

            // Act
            var result = await new SchedulesClient(httpClient)
                .SchedulesPostAsync(new SchedulerStartDto());

            // Assert
            var status = await new StatusClient(httpClient)
                .StatusAsync();
            status.Scheduled.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldReturnError()
        {
            // Arrange
            var httpClient = _service.CreateDefaultClient();
            var client = new NetworksClient(httpClient);

            _service.NetworkPerspectiveCoreMock
                .Setup(x => x.ValidateTokenAsync(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidTokenException("https://networkperspective.io/"));

            // Act
            var exception = await Record.ExceptionAsync(() => client.NetworksPostAsync("foo@networkperspective.io", null));

            // Assert
            (exception as GSuiteClientException).StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }
    }
}