using Microsoft.Graph;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Fixtures
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