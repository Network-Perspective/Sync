using System;
using System.Security;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Domain.Networks.Filters;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Fixtures;
using NetworkPerspective.Sync.Utils.Models;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Services
{
    public class UsersClientTests : IClassFixture<MicrosoftClientBasicFixture>
    {
        private readonly MicrosoftClientBasicFixture _microsoftClientFixture;
        private readonly ILogger<UsersClient> _logger = NullLogger<UsersClient>.Instance;

        public UsersClientTests(MicrosoftClientBasicFixture microsoftClientFixture)
        {
            _microsoftClientFixture = microsoftClientFixture;
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldBeAbleToGetUsers()
        {
            // Arrange
            var client = new UsersClient(_microsoftClientFixture.Client, _logger);
            var timeRange = new TimeRange(new DateTime(2022, 12, 21), new DateTime(2022, 12, 22));
            var filter = new EmployeeFilter(new[] { "group:Sample Team Site" }, Array.Empty<string>());
            var networkConfig = new ConnectorConfig(filter, CustomAttributesConfig.Empty);
            var syncContext = new SyncContext(Guid.NewGuid(), string.Empty, networkConfig, [], new SecureString(), timeRange, Mock.Of<IHashingService>());

            // Act
            var result = await client.GetUsersAsync(syncContext);

            result.Should().NotBeEmpty();
        }
    }
}