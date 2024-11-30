using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Utils.CQS;
using NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

using Xunit;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs;

public class MediatorTests
{
    [Fact]
    public async Task ShouldHandleCommand()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCqs();
        services.AddHandler<CommandHandler, CommandRequest>();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        await mediator.SendAsync(new CommandRequest());

        // Assert

    }

    [Fact]
    public async Task ShouldHandleQuery()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddCqs();
        services.AddHandler<QueryHandler, QueryRequest, Response>();

        // Act
        var serviceProvider = services.BuildServiceProvider();
        var mediator = serviceProvider.GetRequiredService<IMediator>();
        var result = await mediator.SendAsync<QueryRequest, Response>(new QueryRequest());

        // Assert

    }
}
