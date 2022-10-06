namespace NetworkPerspective.Sync.Cli.Tests
{
    public class EntitesTest : IClassFixture<EmbeddedSamplesFixture>
    {
        private readonly Mock<ISyncHashedClient> _coreClient;
        private readonly MockFileSystem _fileSystem;
        private readonly EntitiesClient _entitiesClient;
        private SyncHashedEntitesCommand? _interceptedCommand;

        public EntitesTest(EmbeddedSamplesFixture samples)
        {
            _coreClient = new Mock<ISyncHashedClient>();
            _coreClient.Setup(c => c.SyncEntitiesAsync(It.IsAny<SyncHashedEntitesCommand>()))
                .Returns(Task.FromResult("sample_corellation_id"))
                .Callback<SyncHashedEntitesCommand>(req => _interceptedCommand = req);

            _fileSystem = samples.FileSystem;
            _entitiesClient = new EntitiesClient(_coreClient.Object, _fileSystem);
        }

        [Fact]
        public async Task ItShouldReadAndProcessEntities()
        {
            // Arrange
            var args = new EntitiesOpts()
            {
                Csv = @"entites.csv",
                BaseUrl = "http://localhost",
                Token = "sample_token",
                IdColumns = "EmployeeId",
                CsvDelimiter = "\t",
                RealtionshipColumns = "Supervisor (EmployeeId)",
                ChangeDateColumns = "RowDate"
            };

            // Act
            await _entitiesClient.Main(args);

            // Assert
            var expected = JsonConvert.DeserializeObject<SyncHashedEntitesCommand>(_fileSystem.File.ReadAllText(@"entites-expected.json"));
            _interceptedCommand.Should().BeEquivalentTo(expected);
        }
    }
}