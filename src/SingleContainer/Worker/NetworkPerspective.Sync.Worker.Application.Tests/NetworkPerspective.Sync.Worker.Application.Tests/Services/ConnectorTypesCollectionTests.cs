using System;
using System.Collections.Generic;

using FluentAssertions;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Exceptions;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.Services;

public class ConnectorTypesCollectionTests
{
    [Fact]
    public void ShouldReturnCorrectType()
    {
        // Arrange
        const string name = "name";
        var type = new ConnectorType { Name = name, DataSourceId = "id", DataSourceFacadeFullName = "fullName" };
        var types = new List<ConnectorType> { type };
        var collection = new ConnectorTypesCollection(types);

        // Act
        var actualType = collection[name];

        // Assert
        actualType.Should().BeEquivalentTo(type);
    }

    [Fact]
    public void ShouldThrowOnUknownType()
    {
        // Arrange
        var types = new List<ConnectorType>();
        var collection = new ConnectorTypesCollection(types);

        Func<ConnectorType> func = () => collection["foo"];

        // Act Assert
        func.Should().ThrowExactly<InvalidConnectorTypeException>();
    }

    [Fact]
    public void ShouldReturnTypesNames()
    {
        // Arrange
        const string name1 = "name1";
        const string name2 = "name2";
        var type1 = new ConnectorType { Name = name1, DataSourceId = "id1", DataSourceFacadeFullName = "fullName1" };
        var type2 = new ConnectorType { Name = name2, DataSourceId = "id2", DataSourceFacadeFullName = "fullName2" };
        var types = new List<ConnectorType> { type1, type2 };
        var collection = new ConnectorTypesCollection(types);

        // Act
        var typesNames = collection.GetTypesNames();

        // Assert
        typesNames.Should().BeEquivalentTo([name1, name2]);
    }
}