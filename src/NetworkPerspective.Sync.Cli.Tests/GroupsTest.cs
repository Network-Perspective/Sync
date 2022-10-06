namespace NetworkPerspective.Sync.Cli.Tests
{
    public class GroupsTest : IClassFixture<EmbeddedSamplesFixture>
    {
        private readonly Mock<ISyncHashedClient> _coreClient;
        private readonly MockFileSystem _fileSystem;
        private readonly GroupsClient _entitiesClient;
        private SyncHashedGroupStructureCommand? _interceptedCommand;

        public GroupsTest(EmbeddedSamplesFixture samples)
        {
            _coreClient = new Mock<ISyncHashedClient>();
            _coreClient.Setup(c => c.SyncGroupsAsync(It.IsAny<SyncHashedGroupStructureCommand>()))
                .Returns(Task.FromResult("sample_corellation_id"))
                .Callback<SyncHashedGroupStructureCommand>(req => _interceptedCommand = req);

            _fileSystem = samples.FileSystem;
            _entitiesClient = new GroupsClient(_coreClient.Object, _fileSystem);
        }

        [Fact]
        public async Task ItShouldReadAndProcessGroups()
        {
            // Arrange
            var args = new GroupsOpts()
            {
                Csv = @"C:\groups.csv",
                BaseUrl = "http://localhost",
                Token = "sample_token",
                CsvDelimiter = "\t",
                IdCol = "Code",
                NameCol = "Name",
                CategoryCol = "Category",
                ParentCol = "ParentCode"
            };

            // Act
            await _entitiesClient.Main(args);

            // Assert
            var expected = JsonConvert.DeserializeObject<SyncHashedGroupStructureCommand>(_fileSystem.File.ReadAllText(@"C:\groups-expected.json"));
            _interceptedCommand.Should().BeEquivalentTo(expected);
        }
    }
}