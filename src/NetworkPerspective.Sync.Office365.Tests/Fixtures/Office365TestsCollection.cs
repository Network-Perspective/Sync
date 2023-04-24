using NetworkPerspective.Sync.Common.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Office365.Tests.Fixtures
{
    [CollectionDefinition(Name)]
    public class Office365TestsCollection : ICollectionFixture<InMemoryHostedServiceFixture<Startup>>
    {
        public const string Name = "Office365TestsCollection";
    }
}