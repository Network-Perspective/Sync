using System;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using Moq;

using NetworkPerspective.Sync.Utils.CQS;
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
            await mediator.SendAsync(new CommandRequest());

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
            await mediator.SendAsync(request);

            // Assert
            middlewareMock.Verify(x => x.HandleAsync(request, It.IsAny<Func<CommandRequest, CancellationToken, Task>>(), It.IsAny<CancellationToken>()), Times.Once);
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
            var result = await mediator.SendAsync<QueryRequest, Response>(new QueryRequest());

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
            services
                .AddCqs()
                .AddMiddleware(middlewareMock.Object)
                .AddHandler<QueryHandler, QueryRequest, Response>();

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            var result = await mediator.SendAsync<QueryRequest, Response>(request);

            // Assert
            middlewareMock.Verify(x => x.HandleAsync(request, It.IsAny<Func<QueryRequest, CancellationToken, Task<Response>>>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
