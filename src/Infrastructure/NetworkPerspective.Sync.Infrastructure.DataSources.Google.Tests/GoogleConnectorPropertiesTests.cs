using System.Collections.Generic;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests;

public class GoogleConnectorPropertiesTests
{
    [Fact]
    public void ShouldRequireAdminEmail()
    {
        // Arrange
        var props = new GoogleConnectorProperties(new Dictionary<string, string>
        {
            { nameof(GoogleConnectorProperties.AdminEmail), string.Empty }
        });

        // Act
        var validationResult = new GoogleConnectorProperties.Validator().Validate(props);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, x => x.PropertyName == nameof(GoogleConnectorProperties.AdminEmail));
    }

    [Fact]
    public void ShouldNotAllowToSyncChannelNamesWhenDisabledSyncGroups()
    {
        // Arrange
        var props = new GoogleConnectorProperties(new Dictionary<string, string>
        {
            { nameof(GoogleConnectorProperties.SyncGroups), false.ToString() },
            { nameof(GoogleConnectorProperties.SyncChannelsNames), true.ToString() }
        });

        // Act
        var validationResult = new GoogleConnectorProperties.Validator().Validate(props);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, x => x.PropertyName == nameof(GoogleConnectorProperties.SyncChannelsNames));
    }

    [Fact]
    public void ShouldNotAllowToSyncInteractionsWhenEnabledUserToken()
    {
        // Arrange
        var props = new GoogleConnectorProperties(new Dictionary<string, string>
        {
            { nameof(GoogleConnectorProperties.SyncInteractions), true.ToString() },
            { nameof(GoogleConnectorProperties.UseUserToken), true.ToString() }
        });

        // Act
        var validationResult = new GoogleConnectorProperties.Validator().Validate(props);

        // Assert
        Assert.False(validationResult.IsValid);
        Assert.Contains(validationResult.Errors, x => x.PropertyName == nameof(GoogleConnectorProperties.SyncInteractions));
    }
}