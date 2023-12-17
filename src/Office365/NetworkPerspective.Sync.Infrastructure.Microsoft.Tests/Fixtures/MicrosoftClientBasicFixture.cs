using Microsoft.Graph;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Fixtures
{
    public class MicrosoftClientBasicFixture
    {
        public GraphServiceClient Client { get; }

        public MicrosoftClientBasicFixture()
        {
            Client = ClientFactory.Create(false);
        }
    }
}
