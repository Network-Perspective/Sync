using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests.Fixtures;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests.Services;

public class MemberProfilesClientTests(GoogleClientFixture googleClientFixture) : IClassFixture<GoogleClientFixture>
{
    [Fact]
    [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
    public async Task ShouldGetNonEmptyUserCollection()
    {
        // Arrange
        var googleConfig = new GoogleConfig
        {
            ApplicationName = GoogleClientFixture.ApplicationName,
        };

        var retryPolicyProvider = new RetryPolicyProvider(NullLogger<RetryPolicyProvider>.Instance);
        var credentials = await googleClientFixture.CredentialProvider.ImpersonificateAsync(GoogleClientFixture.AdminEmail);
        var client = new UsersClient(Mock.Of<IScopedStatusCache>(), Options.Create(googleConfig), retryPolicyProvider, NullLogger<UsersClient>.Instance);

        // Act
        var result = await client.GetUsersAsync(credentials);

        // Assert
        result.Should().NotBeNullOrEmpty();
    }
}