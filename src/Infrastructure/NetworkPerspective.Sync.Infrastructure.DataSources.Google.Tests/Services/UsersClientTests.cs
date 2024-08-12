using System;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Criterias;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests.Fixtures;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests.Services
{
    public class MemberProfilesClientTests : IClassFixture<GoogleClientFixture>
    {
        private readonly GoogleClientFixture _googleClientFixture;

        public MemberProfilesClientTests(GoogleClientFixture googleClientFixture)
        {
            _googleClientFixture = googleClientFixture;
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldGetNonEmptyUserCollection()
        {
            // Arrange
            var networkProperties = new GoogleNetworkProperties("nptestuser12@worksmartona.com", null);
            var googleConfig = new GoogleConfig
            {
                ApplicationName = "gmail_app",
            };

            var retryPolicyProvider = new RetryPolicyProvider(NullLogger<RetryPolicyProvider>.Instance);
            var client = new UsersClient(Mock.Of<ITasksStatusesCache>(), Options.Create(googleConfig), Array.Empty<ICriteria>(), retryPolicyProvider, _googleClientFixture.CredentialProvider, NullLogger<UsersClient>.Instance);

            // Act
            var result = await client.GetUsersAsync(Guid.NewGuid(), networkProperties, ConnectorConfig.Empty);

            // Assert
            result.Should().NotBeNullOrEmpty();
        }

    }
}