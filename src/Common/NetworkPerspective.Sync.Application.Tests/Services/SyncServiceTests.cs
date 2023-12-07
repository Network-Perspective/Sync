using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class SyncServiceTests
    {
        private readonly ILogger<SyncService> _logger = NullLogger<SyncService>.Instance;
        private readonly Mock<INetworkPerspectiveCore> _networkPerspectiveCoreMock = new Mock<INetworkPerspectiveCore>();
        private readonly Mock<IInteractionsFilterFactory> _interactionsFilterFactoryMock = new Mock<IInteractionsFilterFactory>();
        private readonly Mock<IInteractionsFilter> _interactionsFilterMock = new Mock<IInteractionsFilter>();
        private readonly Mock<IDataSource> _dataSourceMock = new Mock<IDataSource>();
        private readonly TestableInteractionStream _interactionsStream = new TestableInteractionStream();

        public SyncServiceTests()
        {
            _networkPerspectiveCoreMock.Reset();
            _interactionsFilterFactoryMock.Reset();
            _interactionsFilterMock.Reset();
            _dataSourceMock.Reset();
            _interactionsStream.Reset();

            _networkPerspectiveCoreMock
                .Setup(x => x.OpenInteractionsStream(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .Returns(_interactionsStream);

            _interactionsFilterFactoryMock
                .Setup(x => x.CreateInteractionsFilter(It.IsAny<TimeRange>()))
                .Returns(_interactionsFilterMock.Object);

            _interactionsFilterMock
                .Setup(x => x.Filter(It.IsAny<IEnumerable<Interaction>>()))
                .Returns(new HashSet<Interaction>());
        }

        [Fact]
        [Trait(TestsConsts.TraitTestKind, TestsConsts.TraitAcceptance)]
        public async Task ShouldReportSyncStartedAndSuccessToNpCoreApp()
        {
            // Arrange
            var start = new DateTime(2022, 01, 01);
            var end = new DateTime(2022, 01, 02);
            var timeRange = new TimeRange(start, end);
            var context = new SyncContext(Guid.NewGuid(), NetworkConfig.Empty, new NetworkProperties(), "foo".ToSecureString(), timeRange, Mock.Of<IStatusLogger>(), Mock.Of<IHashingService>());
            var syncService = new SyncService(_logger, _dataSourceMock.Object, Mock.Of<ISyncHistoryService>(), _networkPerspectiveCoreMock.Object, _interactionsFilterFactoryMock.Object, new Clock());

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
            var context = new SyncContext(Guid.NewGuid(), NetworkConfig.Empty, new NetworkProperties(), "foo".ToSecureString(), timeRange, Mock.Of<IStatusLogger>(), Mock.Of<IHashingService>());

            _dataSourceMock
                .Setup(x => x.SyncInteractionsAsync(It.IsAny<IInteractionsStream>(), context, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception());

            var syncService = new SyncService(_logger, _dataSourceMock.Object, Mock.Of<ISyncHistoryService>(), _networkPerspectiveCoreMock.Object, _interactionsFilterFactoryMock.Object, new Clock());

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
            var context = new SyncContext(Guid.NewGuid(), NetworkConfig.Empty, new NetworkProperties(), "foo".ToSecureString(), timeRange, Mock.Of<IStatusLogger>(), Mock.Of<IHashingService>());

            var syncService = new SyncService(_logger, _dataSourceMock.Object, Mock.Of<ISyncHistoryService>(), _networkPerspectiveCoreMock.Object, _interactionsFilterFactoryMock.Object, new Clock());

            // Act
            await syncService.SyncAsync(context);

            // Assert
            _dataSourceMock
                .Verify(x => x.SyncInteractionsAsync(It.Is<IInteractionsStream>(x => x.GetType() == typeof(FilteredInteractionStreamDecorator)), context, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ShouldDisposeStream()
        {
            // Arrange
            var start = new DateTime(2022, 01, 01);
            var end = new DateTime(2022, 01, 02);
            var timeRange = new TimeRange(start, end);
            var context = new SyncContext(Guid.NewGuid(), NetworkConfig.Empty, new NetworkProperties(), "foo".ToSecureString(), timeRange, Mock.Of<IStatusLogger>(), Mock.Of<IHashingService>());

            var interactionsStreamMock = new Mock<IInteractionsStream>();
            _networkPerspectiveCoreMock
                .Setup(x => x.OpenInteractionsStream(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .Returns(interactionsStreamMock.Object);

            var syncService = new SyncService(_logger, _dataSourceMock.Object, Mock.Of<ISyncHistoryService>(), _networkPerspectiveCoreMock.Object, _interactionsFilterFactoryMock.Object, new Clock());

            // Act
            await syncService.SyncAsync(context);

            // Assert
            interactionsStreamMock.Verify(x => x.DisposeAsync(), Times.Once);
        }
    }
}