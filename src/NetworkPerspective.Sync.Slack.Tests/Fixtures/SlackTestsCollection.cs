using NetworkPerspective.Sync.Common.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Slack.Tests.Fixtures
{
    [CollectionDefinition(Name)]
    public class SlackTestsCollection : ICollectionFixture<InMemoryHostedServiceFixture<Startup>>
    {
        public const string Name = "SlackTestsCollection";
    }
}