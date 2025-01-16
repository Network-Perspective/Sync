using System.Collections.Generic;
using System.Collections.Immutable;

using FluentAssertions;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.Domain.Networks;

public class ConnectorPropertiesTests
{
    private readonly TestableConnectorProperties _defaultProps = new(ImmutableDictionary<string, string>.Empty);

    [Fact]
    public void ShouldBindNewProps()
    {
        // Arrange
        const string stringPropValue = "foo";
        const int intPropValue = 42;
        const bool boolPropValue = true;

        var props = new Dictionary<string, string>
        {
            { nameof(TestableConnectorProperties.IntProp), intPropValue.ToString() },
            { nameof(TestableConnectorProperties.StringProp), stringPropValue },
            { nameof(TestableConnectorProperties.BoolProp), boolPropValue.ToString() }
        };

        // Act
        var connectorProperties = new TestableConnectorProperties(props);

        // Assert
        connectorProperties.IntProp.Should().Be(intPropValue);
        connectorProperties.StringProp.Should().Be(stringPropValue);
        connectorProperties.BoolProp.Should().Be(boolPropValue);
    }

    [Fact]
    public void ShouldAllowOverrideDefaultProps()
    {
        // Arrange

        // Act
        var connectorProperties = new TestableConnectorProperties(ImmutableDictionary<string, string>.Empty);

        // Assert
        connectorProperties.SyncEmployees.Should().Be(_defaultProps.SyncEmployees);
    }

    [Fact]
    public void Should()
    {
        // Arrange
        var props = new Dictionary<string, string>
        {
            { nameof(ConnectorProperties.SyncEmployees), (!_defaultProps.SyncEmployees).ToString() },
            { nameof(ConnectorProperties.SyncHashedEmployees), (!_defaultProps.SyncHashedEmployees).ToString() },
            { nameof(ConnectorProperties.SyncGroups), (!_defaultProps.SyncGroups).ToString() },
            { nameof(ConnectorProperties.SyncInteractions), (!_defaultProps.SyncInteractions).ToString() },
            { nameof(ConnectorProperties.SyncChannelsNames), (!_defaultProps.SyncChannelsNames).ToString() },
        };

        // Act
        var connectorProperties = new TestableConnectorProperties(props);

        // Assert
        connectorProperties.SyncEmployees.Should().Be(!_defaultProps.SyncEmployees);
        connectorProperties.SyncHashedEmployees.Should().Be(!_defaultProps.SyncHashedEmployees);
        connectorProperties.SyncGroups.Should().Be(!_defaultProps.SyncGroups);
        connectorProperties.SyncInteractions.Should().Be(!_defaultProps.SyncInteractions);
        connectorProperties.SyncChannelsNames.Should().Be(!_defaultProps.SyncChannelsNames);
    }
}