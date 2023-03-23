﻿using System;
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
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Services
{
    public class UsersClientTests : IClassFixture<MicrosoftClientFixture>
    {
        private readonly MicrosoftClientFixture _microsoftClientFixture;
        private readonly ILogger<UsersClient> _logger = NullLogger<UsersClient>.Instance;

        public UsersClientTests(MicrosoftClientFixture microsoftClientFixture)
        {
            _microsoftClientFixture = microsoftClientFixture;
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldBeAbleToGetUsers()
        {
            // Arrange
            var client = new UsersClient(_microsoftClientFixture.GraphServiceClient, _logger);
            var timeRange = new TimeRange(new DateTime(2022, 12, 21), new DateTime(2022, 12, 22));
            var syncContext = new SyncContext(Guid.NewGuid(), NetworkConfig.Empty, new NetworkProperties(), new SecureString(), timeRange, Mock.Of<IStatusLogger>(), Mock.Of<IHashingService>());

            // Act
            var result = await client.GetUsersAsync(syncContext);

            result.Should().NotBeEmpty();
        }
    }
}