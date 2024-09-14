using System;

using FluentAssertions;

using NetworkPerspective.Sync.Worker.Application.Exceptions;
using NetworkPerspective.Sync.Worker.Application.Mappers;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.Mappers;

public class ConnectorTypeMapperTests
{
    [Theory]
    [InlineData("Google", "GSuiteId")]
    [InlineData("Slack", "SlackId")]
    [InlineData("Excel", "ExcelId")]
    [InlineData("Office365", "Office365Id")]
    public void ShouldMap(string connectorType, string expectedDataSourceIdName)
    {
        // Act
        var actualDataSourceIdName = ConnectorTypeMapper.ToDataSourceId(connectorType);

        // Assert
        actualDataSourceIdName.Should().Be(expectedDataSourceIdName);
    }

    [Fact]
    public void ShouldThrowOnUnknown()
    {
        // Arrange
        Func<string> func = () => ConnectorTypeMapper.ToDataSourceId("unknown type");

        // Act Assert
        func.Should().Throw<InvalidConnectorTypeException>();
    }
}