using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Utils.CQS;
using NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

using Xunit;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs;

public class MediatorTests
{
    public class Request : MediatorTests
    {
        [Fact]
        public async Task ShouldHandleRequest()
        {
            // Arrange
            var services = new ServiceCollection();
            services
                .AddCqs()
                .AddMiddleware<TestableMiddleware>()
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
            TestableMiddleware.Reset();
            var request = new QueryRequest();
            var services = new ServiceCollection();
            services
                .AddCqs()
                .AddMiddleware<TestableMiddleware>()
                .AddHandler<QueryHandler, QueryRequest, Response>();

            // Act
            var serviceProvider = services.BuildServiceProvider();
            var mediator = serviceProvider.GetRequiredService<IMediator>();
            _ = await mediator.SendAsync<QueryRequest, Response>(request);

            // Assert
            TestableMiddleware.CalledCount.Should().Be(1);
        }
    }
}
