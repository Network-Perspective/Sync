using NetworkPerspective.Sync.Infrastructure.Core.HttpClients;

namespace NetworkPerspective.Sync.Cli.Tests
{
    public class GroupsTest : IClassFixture<EmbeddedSamplesFixture>
    {
        private readonly Mock<ISyncHashedClient> _coreClient;
        private readonly MockFileSystem _fileSystem;
        private SyncHashedGroupStructureCommand? _interceptedCommand;

        public GroupsTest(EmbeddedSamplesFixture samples)
        {
            _coreClient = new Mock<ISyncHashedClient>();
            _coreClient.Setup(c => c.SyncGroupsAsync(It.IsAny<SyncHashedGroupStructureCommand>()))
                .Returns(Task.FromResult("sample_corellation_id"))
                .Callback<SyncHashedGroupStructureCommand>(req => _interceptedCommand = req);

            _fileSystem = samples.FileSystem;
        }

        [Fact]
        public async Task ItShouldReadAndProcessGroups()
        {
            // Arrange
            var options = new GroupsOpts()
            {
                Csv = @"groups.csv",
                BaseUrl = "http://localhost",
                Token = "sample_token",
                CsvDelimiter = "\t",
                IdCol = "Code",
                NameCol = "Name",
                CategoryCol = "Category",
                ParentCol = "ParentCode"
            };

            var groupsClient = new GroupsClient(_coreClient.Object, _fileSystem, options);

            // Act
            await groupsClient.Main();

            // Assert
            var expected = JsonConvert.DeserializeObject<SyncHashedGroupStructureCommand>(_fileSystem.File.ReadAllText(@"groups-expected.json"));
            _interceptedCommand.Should().BeEquivalentTo(expected);
        }


        [Fact]
        public async Task ItShouldReadAndProcessGroupsWithClientId()
        {
            // Arrange
            var options = new GroupsOpts()
            {
                Csv = @"groups-with-clientid.csv",
                BaseUrl = "http://localhost",
                Token = "sample_token",
                CsvDelimiter = "\t",
                IdCol = "Code",
                NameCol = "Name",
                CategoryCol = "Category",
                ParentCol = "ParentCode",
                ClientGroupIdCol = "ClientGroupId"
            };

            var groupsClient = new GroupsClient(_coreClient.Object, _fileSystem, options);

            // Act
            await groupsClient.Main();

            // Assert
            var expected = JsonConvert.DeserializeObject<SyncHashedGroupStructureCommand>(_fileSystem.File.ReadAllText(@"groups-with-clientid-expected.json"));
            _interceptedCommand.Should().BeEquivalentTo(expected);
        }
    }
}