using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
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
        private ILogger<SyncService> _logger = NullLogger<SyncService>.Instance;
        private readonly Mock<INetworkPerspectiveCore> _networkPerspectiveCoreMock = new Mock<INetworkPerspectiveCore>();
        private readonly Mock<IInteractionsFilterFactory> _interactionsFilterFactoryMock = new Mock<IInteractionsFilterFactory>();
        private readonly Mock<IInteractionsFilter> _interactionsFilterMock = new Mock<IInteractionsFilter>();
        private readonly Mock<IDataSource> _dataSourceMock = new Mock<IDataSource>();

        public SyncServiceTests()
        {
            _networkPerspectiveCoreMock.Reset();
            _interactionsFilterFactoryMock.Reset();
            _interactionsFilterMock.Reset();
            _dataSourceMock.Reset();

            _interactionsFilterFactoryMock
                .Setup(x => x.CreateInteractionsFilter(It.IsAny<TimeRange>()))
                .Returns(_interactionsFilterMock.Object);

            _dataSourceMock
                .Setup(x => x.GetInteractions(It.IsAny<SyncContext>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HashSet<Interaction>());

            _interactionsFilterMock
                .Setup(x => x.Filter(It.IsAny<IEnumerable<Interaction>>()))
                .Returns(new HashSet<Interaction>());
        }

        public class SyncInteractions : SyncServiceTests
        {
            [Fact]
            [Trait(TestsConsts.TraitTestKind, TestsConsts.TraitAcceptance)]
            public async Task ShouldReportSyncStartedAndSuccessToNpCoreApp()
            {
                // Arrange
                var start = new DateTime(2022, 01, 01);
                var end = new DateTime(2022, 01, 02);
                var context = new SyncContext(Guid.NewGuid(), NetworkConfig.Empty, "foo".ToSecureString(), start, end);
                var syncService = new SyncService(_logger, _dataSourceMock.Object, Mock.Of<ISyncHistoryService>(), _networkPerspectiveCoreMock.Object, _interactionsFilterFactoryMock.Object, Mock.Of<IStatusLogger>(), new Clock());

                // Act
                await syncService.SyncInteractionsAsync(context);

                // Assert
                _networkPerspectiveCoreMock.Verify(x => x.ReportSyncStartAsync(It.IsAny<SecureString>(), context.CurrentRange, It.IsAny<CancellationToken>()), Times.Once);
                _networkPerspectiveCoreMock.Verify(x => x.ReportSyncSuccessfulAsync(It.IsAny<SecureString>(), context.CurrentRange, It.IsAny<CancellationToken>()), Times.Once);
            }
        }
    }
}