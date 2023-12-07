namespace NetworkPerspective.Sync.Cli.Tests
{
    public class SignalTest
    {
        private readonly Mock<ISyncHashedClient> _coreClient;
        private ReportSyncStartedCommand? _syncStarted;
        private ReportSyncCompletedCommand? _syncCompleted;

        public SignalTest()
        {
            _coreClient = new Mock<ISyncHashedClient>();
            _coreClient.Setup(c => c.ReportStartAsync(It.IsAny<ReportSyncStartedCommand>()))
                .Callback<ReportSyncStartedCommand>(req => _syncStarted = req);
            _coreClient.Setup(c => c.ReportCompletedAsync(It.IsAny<ReportSyncCompletedCommand>()))
                .Callback<ReportSyncCompletedCommand>(req => _syncCompleted = req);
        }

        [Fact]
        public async Task ItShouldSignalSyncStarted()
        {
            // Arrange
            var options = new SignalOpts()
            {
                BaseUrl = "http://localhost",
                Token = "sample_token",
                PeriodStart = "2023-02-01",
                PeriodEnd = "2023-03-01",
                TimeZone = "UTC",
                Action = SignalledAction.SyncStart
            };

            var client = new SignalClient(_coreClient.Object, options);


            // Act
            await client.Main();

            // Assert
            var expected = new ReportSyncStartedCommand()
            {
                ServiceToken = "sample_token",
                SyncPeriodStart = new DateTimeOffset(2023, 02, 01, 0, 0, 0, TimeSpan.Zero),
                SyncPeriodEnd = new DateTimeOffset(2023, 03, 01, 0, 0, 0, TimeSpan.Zero)
            };
            _syncStarted.Should().BeEquivalentTo(expected);
        }


        [Fact]
        public async Task ItShouldSignalSyncCompleted()
        {
            // Arrange
            var options = new SignalOpts()
            {
                BaseUrl = "http://localhost",
                Token = "sample_token",
                PeriodStart = "2023-02-01",
                PeriodEnd = "2023-03-01",
                TimeZone = "UTC",
                Action = SignalledAction.SyncCompleted
            };

            var client = new SignalClient(_coreClient.Object, options);


            // Act
            await client.Main();

            // Assert
            var expected = new ReportSyncCompletedCommand()
            {
                ServiceToken = "sample_token",
                SyncPeriodStart = new DateTimeOffset(2023, 02, 01, 0, 0, 0, TimeSpan.Zero),
                SyncPeriodEnd = new DateTimeOffset(2023, 03, 01, 0, 0, 0, TimeSpan.Zero),
                Success = true,
            };
            _syncCompleted.Should().BeEquivalentTo(expected);
        }


        [Fact]
        public async Task ItShouldSignalSyncError()
        {
            // Arrange
            var options = new SignalOpts()
            {
                BaseUrl = "http://localhost",
                Token = "sample_token",
                PeriodStart = "2023-02-01",
                PeriodEnd = "2023-03-01",
                TimeZone = "UTC",
                Action = SignalledAction.SyncError,
                ErrorMessage = "Err"
            };

            var client = new SignalClient(_coreClient.Object, options);


            // Act
            await client.Main();

            // Assert
            var expected = new ReportSyncCompletedCommand()
            {
                ServiceToken = "sample_token",
                SyncPeriodStart = new DateTimeOffset(2023, 02, 01, 0, 0, 0, TimeSpan.Zero),
                SyncPeriodEnd = new DateTimeOffset(2023, 03, 01, 0, 0, 0, TimeSpan.Zero),
                Success = false,
                Message = "Err"
            };
            _syncCompleted.Should().BeEquivalentTo(expected);
        }
    }
}