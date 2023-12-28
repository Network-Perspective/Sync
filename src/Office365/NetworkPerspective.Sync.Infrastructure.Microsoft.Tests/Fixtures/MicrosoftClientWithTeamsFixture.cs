using Microsoft.Graph;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Fixtures
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