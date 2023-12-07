using NetworkPerspective.Sync.Common.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.GSuite.Tests.Fixtures
{
    [CollectionDefinition(Name)]
    public class GSuiteTestsCollection : ICollectionFixture<InMemoryHostedServiceFixture<Startup>>
    {
        public const string Name = "GSuiteTestsCollection";
    }
}