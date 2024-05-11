
using NetworkPerspective.Sync.Orchestrator.Tests;

using Xunit;

namespace NetworkPerspective.Sync.Office365.Tests.Fixtures
{
    [CollectionDefinition(Name)]
    public class TestsCollection : ICollectionFixture<OrchestratorServiceFixture>
    {
        public const string Name = "OrchestratorTestsCollection";
    }
}