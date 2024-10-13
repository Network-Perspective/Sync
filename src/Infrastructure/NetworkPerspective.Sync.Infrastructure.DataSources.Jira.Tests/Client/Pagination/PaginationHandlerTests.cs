using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Pagination;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Tests.Client.Pagination;

public class PaginationHandlerTests
{
    private readonly PaginationHandler _paginationHandler;

    public PaginationHandlerTests()
    {
        _paginationHandler = new PaginationHandler(new NullLogger<PaginationHandler>());
    }

    [Fact]
    public async Task ShouldReturnAllEntitiesWhenSinglePageResponse()
    {
        // Arrange
        var mockResponse = new Mock<IPaginatedResponse<string>>();
        mockResponse
            .Setup(r => r.Values)
            .Returns(["Entity1", "Entity2"]);
        mockResponse
            .Setup(r => r.IsLast)
            .Returns(true); // Last page

        Task<IPaginatedResponse<string>> CallApi(int startAt, CancellationToken token)
            => Task.FromResult(mockResponse.Object);

        // Act
        var result = await _paginationHandler.GetAllAsync<string, IPaginatedResponse<string>>(CallApi);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().ContainInOrder("Entity1", "Entity2");
    }

    [Fact]
    public async Task ShouldReturnAllEntitiesWhenMultiplePagesResponse()
    {
        // Arrange
        var mockResponse1 = new Mock<IPaginatedResponse<string>>();
        mockResponse1
            .Setup(r => r.Values)
            .Returns(["Entity1", "Entity2"]);
        mockResponse1
            .Setup(r => r.IsLast)
            .Returns(false); // Not the last page

        var mockResponse2 = new Mock<IPaginatedResponse<string>>();
        mockResponse2
            .Setup(r => r.Values)
            .Returns(["Entity3", "Entity4"]);
        mockResponse2
            .Setup(r => r.IsLast)
            .Returns(true); // Last page

        int callCount = 0;
        Task<IPaginatedResponse<string>> CallApi(int startAt, CancellationToken token)
        {
            callCount++;
            return Task.FromResult(callCount == 1 ? mockResponse1.Object : mockResponse2.Object);
        }

        // Act
        var result = await _paginationHandler.GetAllAsync<string, IPaginatedResponse<string>>(CallApi);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(4);
        result.Should().ContainInOrder("Entity1", "Entity2", "Entity3", "Entity4");
    }

    [Fact]
    public async Task ShouldStopWhenCancellationIsRequested()
    {
        // Arrange
        var mockResponse = new Mock<IPaginatedResponse<string>>();
        mockResponse
            .Setup(r => r.Values)
            .Returns(["Entity1", "Entity2"]);
        mockResponse
            .Setup(r => r.IsLast)
            .Returns(false); // Not the last page

        var cts = new CancellationTokenSource();

        Task<IPaginatedResponse<string>> CallApi(int startAt, CancellationToken token)
        {
            cts.Cancel(); // Cancel after the first call
            return Task.FromResult(mockResponse.Object);
        }

        // Act
        var result = await _paginationHandler.GetAllAsync<string, IPaginatedResponse<string>>(CallApi, cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().ContainInOrder("Entity1", "Entity2");
    }

    [Fact]
    public async Task ShouldReturnEmptyListWhenApiReturnsNoEntities()
    {
        // Arrange
        var mockResponse = new Mock<IPaginatedResponse<string>>();
        mockResponse
            .Setup(r => r.Values)
            .Returns([]);
        mockResponse
            .Setup(r => r.IsLast)
            .Returns(true); // Last page

        Task<IPaginatedResponse<string>> CallApi(int startAt, CancellationToken token)
            => Task.FromResult(mockResponse.Object);

        // Act
        var result = await _paginationHandler.GetAllAsync<string, IPaginatedResponse<string>>(CallApi);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}