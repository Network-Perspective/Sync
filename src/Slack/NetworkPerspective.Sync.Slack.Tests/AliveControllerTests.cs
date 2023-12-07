using System.Net;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Common.Tests.Fixtures;
using NetworkPerspective.Sync.Slack.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Slack.Tests
{
    [Collection(SlackTestsCollection.Name)]
    public class AliveControllerTests
    {
        private readonly InMemoryHostedServiceFixture<Startup> _service;

        public AliveControllerTests(InMemoryHostedServiceFixture<Startup> service)
        {
            _service = service;
            service.Reset();
        }

        [Fact]
        public async Task ShouldReturnOkOnAliveEndpoint()
        {
            // Arrange
            var httpClient = _service.CreateDefaultClient();

            // Act
            var result = await httpClient.GetAsync("/");

            // Arrange
            result.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}