﻿using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Utils.CQS;
using NetworkPerspective.Sync.Utils.CQS.Queries;
using NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

using Xunit;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ShouldResolveQueryHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddCqs()
            .AddHandler<QueryHandler, QueryRequest, Response>();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<IRequestHandler<QueryRequest, Response>>();

        // Assert
        service.Should().NotBeNull();
    }
}