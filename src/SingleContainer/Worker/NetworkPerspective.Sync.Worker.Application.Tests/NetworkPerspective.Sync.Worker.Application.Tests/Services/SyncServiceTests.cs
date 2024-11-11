using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Interactions;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.Services;

public class SyncServiceTests
{
    const string ConnectorTypeName = "Slack";
    const string DataSourceId = "SlackId";
    private readonly ILogger<SyncService> _logger = NullLogger<SyncService>.Instance;
    private readonly Mock<INetworkPerspectiveCore> _networkPerspectiveCoreMock = new();
    private readonly Mock<IInteractionsFilterFactory> _interactionsFilterFactoryMock = new();
    private readonly Mock<IInteractionsFilter> _interactionsFilterMock = new();
    private readonly Mock<IDataSource> _dataSourceMock = new();
    private readonly Mock<ITasksStatusesCache> _tasksStatusesCache = new();
    private readonly TestableInteractionStream _interactionsStream = new();
    private readonly IConnectorTypesCollection _connectorTypes;

    public SyncServiceTests()
    {
        _networkPerspectiveCoreMock.Reset();
        _interactionsFilterFactoryMock.Reset();
        _interactionsFilterMock.Reset();
        _interactionsStream.Reset();
        _dataSourceMock.Reset();
        _tasksStatusesCache.Reset();

        _networkPerspectiveCoreMock
            .Setup(x => x.OpenInteractionsStream(It.IsAny<SecureString>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(_interactionsStream);

        _interactionsFilterFactoryMock
            .Setup(x => x.CreateInteractionsFilter(It.IsAny<TimeRange>()))
            .Returns(_interactionsFilterMock.Object);

        _interactionsFilterMock
            .Setup(x => x.Filter(It.IsAny<IEnumerable<Interaction>>()))
            .Returns(new HashSet<Interaction>());

        var connectorType = new ConnectorType { Name = ConnectorTypeName, DataSourceId = DataSourceId };
        _connectorTypes = new ConnectorTypesCollection([connectorType]);
    }

    [Fact]
    [Trait(TestsConsts.TraitTestKind, TestsConsts.TraitAcceptance)]
    public async Task ShouldReportSyncStartedAndSuccessToNpCoreApp()
    {
        // Arrange
        var start = new DateTime(2022, 01, 01);
        var end = new DateTime(2022, 01, 02);
        var timeRange = new TimeRange(start, end);
        var context = new SyncContext(Guid.NewGuid(), ConnectorTypeName, ConnectorConfig.Empty, [], "foo".ToSecureString(), timeRange, Mock.Of<IHashingService>());

        _dataSourceMock
            .Setup(x => x.SyncInteractionsAsync(It.IsAny<IInteractionsStream>(), context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SyncResult(10, 100, Enumerable.Empty<Exception>()));
        var syncService = new SyncService(_logger, _dataSourceMock.Object, _networkPerspectiveCoreMock.Object, Mock.Of<IStatusLogger>(), _tasksStatusesCache.Object, _interactionsFilterFactoryMock.Object, _connectorTypes);

        // Act
        await syncService.SyncAsync(context);

        // Assert
        _networkPerspectiveCoreMock.Verify(x => x.ReportSyncStartAsync(It.IsAny<SecureString>(), context.TimeRange, It.IsAny<CancellationToken>()), Times.Once);
        _networkPerspectiveCoreMock.Verify(x => x.ReportSyncSuccessfulAsync(It.IsAny<SecureString>(), context.TimeRange, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldAttemptToReportSyncFailedOnException()
    {
        // Arrange
        var start = new DateTime(2022, 01, 01);
        var end = new DateTime(2022, 01, 02);
        var timeRange = new TimeRange(start, end);
        var context = new SyncContext(Guid.NewGuid(), ConnectorTypeName, ConnectorConfig.Empty, [], "foo".ToSecureString(), timeRange, Mock.Of<IHashingService>());

        _dataSourceMock
            .Setup(x => x.SyncInteractionsAsync(It.IsAny<IInteractionsStream>(), context, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception());

        var syncService = new SyncService(_logger, _dataSourceMock.Object, _networkPerspectiveCoreMock.Object, Mock.Of<IStatusLogger>(), _tasksStatusesCache.Object, _interactionsFilterFactoryMock.Object, _connectorTypes);

        // Act
        await syncService.SyncAsync(context);

        // Assert
        _networkPerspectiveCoreMock.Verify(x => x.ReportSyncStartAsync(It.IsAny<SecureString>(), context.TimeRange, It.IsAny<CancellationToken>()), Times.Once);
        _networkPerspectiveCoreMock.Verify(x => x.TryReportSyncFailedAsync(It.IsAny<SecureString>(), context.TimeRange, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldUseFilteredInteractionsStreamDecorator()
    {
        // Arrange
        var start = new DateTime(2022, 01, 01);
        var end = new DateTime(2022, 01, 02);
        var timeRange = new TimeRange(start, end);
        var context = new SyncContext(Guid.NewGuid(), ConnectorTypeName, ConnectorConfig.Empty, [], "foo".ToSecureString(), timeRange, Mock.Of<IHashingService>());

        var syncService = new SyncService(_logger, _dataSourceMock.Object, _networkPerspectiveCoreMock.Object, Mock.Of<IStatusLogger>(), _tasksStatusesCache.Object, _interactionsFilterFactoryMock.Object, _connectorTypes);

        // Act
        await syncService.SyncAsync(context);

        // Assert
        _dataSourceMock
            .Verify(x => x.SyncInteractionsAsync(It.Is<IInteractionsStream>(x => x.GetType() == typeof(FilteredInteractionStreamDecorator)), context, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldFilterChannelGroupsOnDefaultConnectorProperties()
    {
        // Arrange
        var start = new DateTime(2022, 01, 01);
        var end = new DateTime(2022, 01, 02);
        var timeRange = new TimeRange(start, end);
        var connectorProperties = new ConnectorProperties(true, false, null);
        var context = new SyncContext(Guid.NewGuid(), string.Empty, ConnectorConfig.Empty, connectorProperties.GetAll(), "foo".ToSecureString(), timeRange, Mock.Of<IHashingService>());

        var employeeId = EmployeeId.Create("foo", "bar");
        var departmentGroup = Group.Create("group1Id", "groupName1", Group.DepartmentCatergory);
        var channelGroup = Group.Create("group2Id", "groupName2", Group.ChannelCategory);
        var groups = new[] { departmentGroup, channelGroup };
        var employee = Employee.CreateInternal(employeeId, groups);

        _dataSourceMock
            .Setup(x => x.GetHashedEmployeesAsync(context, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployeeCollection([employee], HashFunction.Empty));

        var syncService = new SyncService(_logger, _dataSourceMock.Object, _networkPerspectiveCoreMock.Object, Mock.Of<IStatusLogger>(), _tasksStatusesCache.Object, _interactionsFilterFactoryMock.Object, _connectorTypes);

        // Act
        await syncService.SyncAsync(context);

        // Assert
        _networkPerspectiveCoreMock
            .Verify(
                x => x.PushGroupsAsync(
                    It.IsAny<SecureString>(),
                    It.Is<IEnumerable<Group>>(x => x.Count() == 1 && x.Single().Category != Group.ChannelCategory),
                    It.IsAny<CancellationToken>()),
                Times.Once);
    }

    [Fact]
    public async Task ShouldDisposeStream()
    {
        // Arrange
        var start = new DateTime(2022, 01, 01);
        var end = new DateTime(2022, 01, 02);
        var timeRange = new TimeRange(start, end);
        var context = new SyncContext(Guid.NewGuid(), ConnectorTypeName, ConnectorConfig.Empty, [], "foo".ToSecureString(), timeRange, Mock.Of<IHashingService>());

        var interactionsStreamMock = new Mock<IInteractionsStream>();
        _networkPerspectiveCoreMock
            .Setup(x => x.OpenInteractionsStream(It.IsAny<SecureString>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(interactionsStreamMock.Object);

        var syncService = new SyncService(_logger, _dataSourceMock.Object, _networkPerspectiveCoreMock.Object, Mock.Of<IStatusLogger>(), _tasksStatusesCache.Object, _interactionsFilterFactoryMock.Object, _connectorTypes);

        // Act
        await syncService.SyncAsync(context);

        // Assert
        interactionsStreamMock.Verify(x => x.DisposeAsync(), Times.Once);
    }

    [Fact]
    public async Task ShouldSetTaskStatusToEmptyOnEnd()
    {
        // Arrange
        var start = new DateTime(2022, 01, 01);
        var end = new DateTime(2022, 01, 02);
        var timeRange = new TimeRange(start, end);
        var context = new SyncContext(Guid.NewGuid(), ConnectorTypeName, ConnectorConfig.Empty, [], "foo".ToSecureString(), timeRange, Mock.Of<IHashingService>());

        var syncService = new SyncService(_logger, _dataSourceMock.Object, _networkPerspectiveCoreMock.Object, Mock.Of<IStatusLogger>(), _tasksStatusesCache.Object, _interactionsFilterFactoryMock.Object, _connectorTypes);

        // Act
        await syncService.SyncAsync(context);

        // Assert
        _tasksStatusesCache.Verify(x => x.SetStatusAsync(context.ConnectorId, SingleTaskStatus.Empty, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ShouldSetTaskStatusToNonEmptyOnStart()
    {
        // Arrange
        var start = new DateTime(2022, 01, 01);
        var end = new DateTime(2022, 01, 02);
        var timeRange = new TimeRange(start, end);
        var context = new SyncContext(Guid.NewGuid(), ConnectorTypeName, ConnectorConfig.Empty, [], "foo".ToSecureString(), timeRange, Mock.Of<IHashingService>());

        var syncService = new SyncService(_logger, _dataSourceMock.Object, _networkPerspectiveCoreMock.Object, Mock.Of<IStatusLogger>(), _tasksStatusesCache.Object, _interactionsFilterFactoryMock.Object, _connectorTypes);

        // Act
        await syncService.SyncAsync(context);

        // Assert
        _tasksStatusesCache.Verify(x => x.SetStatusAsync(context.ConnectorId, It.Is<SingleTaskStatus>(x => x != SingleTaskStatus.Empty), It.IsAny<CancellationToken>()), Times.Once);
    }
}