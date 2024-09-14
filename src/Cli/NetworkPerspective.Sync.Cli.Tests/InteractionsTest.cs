using NetworkPerspective.Sync.Infrastructure.Core.HttpClients;

namespace NetworkPerspective.Sync.Cli.Tests
{
    public class InteractionsTest : IClassFixture<EmbeddedSamplesFixture>
    {
        private readonly Mock<ISyncHashedClient> _coreClient;
        private readonly MockFileSystem _fileSystem;
        private readonly List<SyncHashedInteractionsCommand> _interceptedCommand = new List<SyncHashedInteractionsCommand>();

        public InteractionsTest(EmbeddedSamplesFixture samples)
        {
            _coreClient = new Mock<ISyncHashedClient>();
            _coreClient.Setup(c => c.SyncInteractionsAsync(It.IsAny<SyncHashedInteractionsCommand>()))
                .Returns(Task.FromResult("sample_corellation_id"))
                .Callback<SyncHashedInteractionsCommand>(req => _interceptedCommand.Add(req));

            _fileSystem = samples.FileSystem;
        }

        [Fact]
        public async Task ItShouldReadAndProcessInteractions()
        {
            // Arrange
            var options = new InteractionsOpts()
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

            var interactionsClient = new InteractionsClient(_coreClient.Object, _fileSystem, options);

            // Act
            await interactionsClient.Main();

            // Assert
            var expected = JsonConvert.DeserializeObject<SyncHashedInteractionsCommand>(_fileSystem.File.ReadAllText(@"interactions-expected.json"));
            _interceptedCommand[0].Should().BeEquivalentTo(expected);
            _coreClient.Verify(c => c.SyncInteractionsAsync(It.IsAny<SyncHashedInteractionsCommand>()), Times.Once());
        }


        [Fact]
        public async Task ItShouldReadAndProcessInteractionsWithIds()
        {
            // Arrange
            var options = new InteractionsOpts()
            {
                Csv = new[] { @"interactions-with-id.csv" },
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
                InteractionId = "Id",
                BatchSize = 100000,
            };

            var interactionsClient = new InteractionsClient(_coreClient.Object, _fileSystem, options);

            // Act
            await interactionsClient.Main();

            // Assert
            var expected = JsonConvert.DeserializeObject<SyncHashedInteractionsCommand>(_fileSystem.File.ReadAllText(@"interactions-with-id-expected.json"));
            _interceptedCommand[0].Should().BeEquivalentTo(expected);
            _coreClient.Verify(c => c.SyncInteractionsAsync(It.IsAny<SyncHashedInteractionsCommand>()), Times.Once());
        }

        [Fact]
        public async Task ItShouldSendRequestInBatchesOfSpecifiedSize()
        {
            // Arrange
            var options = new InteractionsOpts()
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

            var interactionsClient = new InteractionsClient(_coreClient.Object, _fileSystem, options);

            // Act
            await interactionsClient.Main();

            // Assert
            _coreClient.Verify(c => c.SyncInteractionsAsync(It.IsAny<SyncHashedInteractionsCommand>()), Times.Exactly(3));
            _coreClient.Verify(c => c.SyncInteractionsAsync(It.Is<SyncHashedInteractionsCommand>(x => x.Interactions.Count == 2)), Times.Exactly(2));
            _coreClient.Verify(c => c.SyncInteractionsAsync(It.Is<SyncHashedInteractionsCommand>(x => x.Interactions.Count == 1)), Times.Exactly(1));

            var expectedCommand = JsonConvert.DeserializeObject<SyncHashedInteractionsCommand>(_fileSystem.File.ReadAllText(@"interactions-expected.json"));
            _interceptedCommand
                .SelectMany(x => x.Interactions)
                .Should()
                .BeEquivalentTo(expectedCommand!.Interactions);
        }
    }
}