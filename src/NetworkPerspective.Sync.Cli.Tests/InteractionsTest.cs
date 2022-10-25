using NetworkPerspective.Sync.Infrastructure.Core.Services;

namespace NetworkPerspective.Sync.Cli.Tests
{
    public class InteractionsTest : IClassFixture<EmbeddedSamplesFixture>
    {
        private readonly Mock<ISyncHashedClient> _coreClient;
        private readonly MockFileSystem _fileSystem;
        private readonly InteractionsClient _interactionsClient;
        private readonly List<SyncHashedInteractionsCommand> _interceptedCommand = new List<SyncHashedInteractionsCommand>();

        public InteractionsTest(EmbeddedSamplesFixture samples)
        {
            _coreClient = new Mock<ISyncHashedClient>();
            _coreClient.Setup(c => c.SyncInteractionsAsync(It.IsAny<SyncHashedInteractionsCommand>()))
                .Returns(Task.FromResult("sample_corellation_id"))
                .Callback<SyncHashedInteractionsCommand>(req => _interceptedCommand.Add(req));

            _fileSystem = samples.FileSystem;            
            _interactionsClient = new InteractionsClient(_coreClient.Object, _fileSystem, new InteractionsBatchSplitter());
        }

        [Fact]
        public async Task ItShouldReadAndProcessInteractions()
        {
            // Arrange
            var args = new InteractionsOpts()
            {
                Csv = new[] { @"interactions.csv" },
                BaseUrl = "http://localhost",
                Token = "sample_token",
                CsvDelimiter = "\t",
                FromCol = "From(EmployeeId)",
                ToCol = "To(EmployeeId)",
                WhenCol = "When",
                EventIdCol = "EventId",
                DurationCol = "Duration",
                RecurrentceCol = "RecurrenceType",
                DataSourceType = "Meeting",
                BatchSize = 100000,
            };

            // Act
            await _interactionsClient.Main(args);

            // Assert
            var expected = JsonConvert.DeserializeObject<SyncHashedInteractionsCommand>(_fileSystem.File.ReadAllText(@"interactions-expected.json"));
            _interceptedCommand[0].Should().BeEquivalentTo(expected);
            _coreClient.Verify(c => c.SyncInteractionsAsync(It.IsAny<SyncHashedInteractionsCommand>()), Times.Once());

        }

        [Fact]
        public async Task ItShouldSendRequestInBatchesOfSpecifiedSize()
        {
            // Arrange
            var args = new InteractionsOpts()
            {
                Csv = new[] { @"interactions.csv" },
                BaseUrl = "http://localhost",
                Token = "sample_token",
                CsvDelimiter = "\t",
                FromCol = "From(EmployeeId)",
                ToCol = "To(EmployeeId)",
                WhenCol = "When",
                EventIdCol = "EventId",
                DurationCol = "Duration",
                RecurrentceCol = "RecurrenceType",
                DataSourceType = "Meeting",
                BatchSize = 2,
            };

            // Act
            await _interactionsClient.Main(args);

            // Assert
            _coreClient.Verify(c => c.SyncInteractionsAsync(It.IsAny<SyncHashedInteractionsCommand>()), Times.Exactly(3));

            var batch1 = JsonConvert.DeserializeObject<SyncHashedInteractionsCommand>(_fileSystem.File.ReadAllText(@"interactions-expected-batch-0.json"));
            _interceptedCommand[0].Should().BeEquivalentTo(batch1);

            var batch2 = JsonConvert.DeserializeObject<SyncHashedInteractionsCommand>(_fileSystem.File.ReadAllText(@"interactions-expected-batch-1.json"));
            _interceptedCommand[1].Should().BeEquivalentTo(batch2);

            var batch3 = JsonConvert.DeserializeObject<SyncHashedInteractionsCommand>(_fileSystem.File.ReadAllText(@"interactions-expected-batch-2.json"));
            _interceptedCommand[2].Should().BeEquivalentTo(batch3);
        }


        [Fact]
        public async Task ItShouldNotSplitEventsToDifferentBatches()
        {
            // Arrange
            var args = new InteractionsOpts()
            {
                Csv = new[] { @"interactions-split2.csv" },
                BaseUrl = "http://localhost",
                Token = "sample_token",
                CsvDelimiter = "\t",
                FromCol = "From(EmployeeId)",
                ToCol = "To(EmployeeId)",
                WhenCol = "When",
                EventIdCol = "EventId",
                DurationCol = "Duration",
                RecurrentceCol = "RecurrenceType",
                DataSourceType = "Meeting",
                BatchSize = 2,
            };

            // Act
            await _interactionsClient.Main(args);

            // Assert
            _coreClient.Verify(c => c.SyncInteractionsAsync(It.IsAny<SyncHashedInteractionsCommand>()), Times.Exactly(2));

            var batch1 = JsonConvert.DeserializeObject<SyncHashedInteractionsCommand>(_fileSystem.File.ReadAllText(@"interactions-split2-expected-batch-0.json"));
            _interceptedCommand[0].Should().BeEquivalentTo(batch1);

            var batch2 = JsonConvert.DeserializeObject<SyncHashedInteractionsCommand>(_fileSystem.File.ReadAllText(@"interactions-split2-expected-batch-1.json"));
            _interceptedCommand[1].Should().BeEquivalentTo(batch2);
        }
    }
}