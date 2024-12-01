using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetworkPerspective.Sync.Utils.CQS;
using NetworkPerspective.Sync.Utils.CQS.Commands;
using NetworkPerspective.Sync.Utils.CQS.Middlewares;
using NetworkPerspective.Sync.Utils.CQS.Queries;
using NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

using Xunit;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs;

public class MediatorTests
{
    public class Command : MediatorTests
    {
        [Fact]
        public async Task ShouldHandle()
        {
            // Arrange
            var services = new ServiceCollection();
            services
                .AddCqs()
                .AddMiddleware<NoOpMiddleware>()
                .AddHandler<CommandHandler, CommandRequest>();

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            await mediator.SendCommandAsync(new CommandRequest());

            // Assert
        }

        [Fact]
        public async Task ShouldUseMiddleware()
        {
            // Arrange
            var request = new CommandRequest();
            var services = new ServiceCollection();
            var middlewareMock = new Mock<IMediatorMiddleware>();
            services
                .AddCqs()
                .AddMiddleware(middlewareMock.Object)
                .AddHandler<CommandHandler, CommandRequest>();

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            await mediator.SendCommandAsync(request);

            // Assert
            middlewareMock.Verify(x => x.HandleCommandAsync(request, It.IsAny<CommandHandlerDelegate<CommandRequest>>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    public class Query : MediatorTests
    {
        [Fact]
        public async Task ShouldHandleQuery()
        {
            // Arrange
            var services = new ServiceCollection();
            services
                .AddCqs()
                .AddMiddleware<NoOpMiddleware>()
                .AddHandler<QueryHandler, QueryRequest, Response>();

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var result = await mediator.SendQueryAsync<QueryRequest, Response>(new QueryRequest());

            // Assert
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task ShouldUseMiddleware()
        {
            // Arrange
            var request = new QueryRequest();
            var services = new ServiceCollection();
            var middlewareMock = new Mock<IMediatorMiddleware>();
            ((ICqsBuilder)services
                .AddCqs())
                .AddMiddleware(middlewareMock.Object)
                .AddHandler<QueryHandler, QueryRequest, Response>();

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var result = await mediator.SendQueryAsync<QueryRequest, Response>(request);

            // Assert
            middlewareMock.Verify(x => x.HandleQueryAsync(request, It.IsAny<QueryHandlerDelegate<QueryRequest, Response>>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
