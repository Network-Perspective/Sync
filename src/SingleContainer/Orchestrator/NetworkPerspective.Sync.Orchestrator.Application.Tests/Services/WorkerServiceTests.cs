using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Workers;
using NetworkPerspective.Sync.Orchestrator.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Orchestrator.Application.Tests.Services;

public class WorkerServiceTests
{
    private static readonly ILogger<WorkersService> NullLogger = NullLogger<WorkersService>.Instance;
    private readonly SqliteUnitOfWorkFactory _sqliteUnitOfWorkFactory = new();
    private readonly ICryptoService _cryptoService = new CryptoService();
    private readonly Mock<IWorkerRouter> _workerRouterMock = new();
    private readonly IClock _clock = new Clock();

    public class GetByName : WorkerServiceTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task ShouldReturnOnlineStatus(bool isOnline)
        {
            // Arrange
            const string workerName = "worker-name";

            _workerRouterMock
                .Setup(x => x.IsConnected(workerName))
                .Returns(isOnline);

            var workersService = new WorkersService(_sqliteUnitOfWorkFactory.Create(), _workerRouterMock.Object, _clock, _cryptoService, NullLogger);
            await workersService.CreateAsync(workerName, "foo");

            // Act
            var worker = await workersService.GetAsync(workerName);

            // Assert
            worker.IsOnline.Should().Be(isOnline);
        }
    }

    public class GetAll : WorkerServiceTests
    {
        [Fact]
        public async Task ShouldReturnOnlineStatus()
        {
            // Arrange
            const string workerName1 = "worker-name-1";
            const string workerName2 = "worker-name-2";

            _workerRouterMock
                .Setup(x => x.IsConnected(workerName1))
                .Returns(true);

            _workerRouterMock
                .Setup(x => x.IsConnected(workerName2))
                .Returns(false);

            var workersService = new WorkersService(_sqliteUnitOfWorkFactory.Create(), _workerRouterMock.Object, _clock, _cryptoService, NullLogger);
            await workersService.CreateAsync(workerName1, "secret");
            await workersService.CreateAsync(workerName2, "secret");


            // Act
            var workers = await workersService.GetAllAsync();

            // Assert
            workers.Single(x => x.Name == workerName1).IsOnline.Should().BeTrue();
            workers.Single(x => x.Name == workerName2).IsOnline.Should().BeFalse();
        }
    }
}