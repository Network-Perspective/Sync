using Microsoft.Graph;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Fixtures
{
    public class MicrosoftClientWithTeamsFixture
    {
        public GraphServiceClient Client { get; }

        public MicrosoftClientWithTeamsFixture()
        {
            Client = ClientFactory.Create(true);
        }
    }
}