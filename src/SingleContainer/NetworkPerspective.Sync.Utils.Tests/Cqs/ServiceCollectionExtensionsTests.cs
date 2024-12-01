using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Utils.CQS;
using NetworkPerspective.Sync.Utils.CQS.Commands;
using NetworkPerspective.Sync.Utils.CQS.Queries;
using NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

using Xunit;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void ShouldResolveCommandHandler()
    {
        // Arrange
        var services = new ServiceCollection();
        services
            .AddCqs()
            .AddHandler<CommandHandler, CommandRequest>();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetService<ICommandHandler<CommandRequest>>();

        // Assert
        service.Should().NotBeNull();
    }

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
        var service = serviceProvider.GetService<IQueryHandler<QueryRequest, Response>>();

        // Assert
        service.Should().NotBeNull();
    }
}

