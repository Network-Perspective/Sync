using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

using FluentAssertions;

using Microsoft.AspNetCore.Mvc;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Common.Tests.Fixtures;
using NetworkPerspective.Sync.Infrastructure.Microsoft;
using NetworkPerspective.Sync.Office365.Client;
using NetworkPerspective.Sync.Office365.Tests.Fixtures;

using Newtonsoft.Json;

using Xunit;

namespace NetworkPerspective.Sync.Office365.Tests
{
    [Collection(Office365TestsCollection.Name)]
    public class AuthControllerTests
    {
        private readonly InMemoryHostedServiceFixture<Startup> _service;

        public AuthControllerTests(InMemoryHostedServiceFixture<Startup> service)
        {
            _service = service;
            service.Reset();
        }

        [Fact]
        public async Task ShouldInitializeOAuthWithReturningUrlToMicrosoft()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var clientId = Guid.NewGuid().ToString();
            var httpClient = _service.CreateDefaultClient();

            _service.NetworkPerspectiveCoreMock
                .Setup(x => x.ValidateTokenAsync(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenValidationResponse(networkId, Guid.NewGuid()));

            _service.SecretRepositoryMock
                .Setup(x => x.GetSecretAsync(MicrosoftKeys.MicrosoftClientIdKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId.ToSecureString());

            var networkConfig = new NetworkConfigDto
            {
                SyncMsTeams = false
            };

            var networkCreateResponse = await new NetworksClient(httpClient)
                .NetworksPostAsync(networkConfig);

            // Act
            var response = await new AuthClient(httpClient)
                .AuthAsync(null);

            // Assert
            var responseUri = new Uri(response);
            var resultClientId = HttpUtility.ParseQueryString(responseUri.Query).Get("client_id");
            var resultState = HttpUtility.ParseQueryString(responseUri.Query).Get("state");
            var resultUri = HttpUtility.ParseQueryString(responseUri.Query).Get("redirect_uri");

            resultClientId.Should().Be(clientId);
            Guid.TryParse(resultState, out Guid parsed).Should().BeTrue();
            resultUri.Should().Be("https://localhost/auth/callback");
        }

        [Fact]
        public async Task ShouldHandleOnCorrectResponse()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var clientId = Guid.NewGuid().ToString();
            var httpClient = _service.CreateDefaultClient();

            _service.NetworkPerspectiveCoreMock
                .Setup(x => x.ValidateTokenAsync(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenValidationResponse(networkId, Guid.NewGuid()));

            _service.SecretRepositoryMock
                .Setup(x => x.GetSecretAsync(MicrosoftKeys.MicrosoftClientIdKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId.ToSecureString());

            var networkConfig = new NetworkConfigDto
            {
                SyncMsTeams = false
            };

            var networkCreateResponse = await new NetworksClient(httpClient)
                .NetworksPostAsync(networkConfig);

            var authClient = new AuthClient(httpClient);

            var authResponse = await authClient
                .AuthAsync(null);

            var state = HttpUtility.ParseQueryString(new Uri(authResponse).Query).Get("state");

            // Act
            var authCallbackResponse = await authClient
                .CallbackAsync(tenantId, state, null, null);

            var tenantKey = string.Format(MicrosoftKeys.MicrosoftTenantIdPattern, networkId.ToString());
            _service.SecretRepositoryMock.Verify(x => x.SetSecretAsync(tenantKey, It.Is<SecureString>(x => x.ToSystemString() == tenantId.ToString()), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task ShouldReturnProblemDetailsOnReturnedError()
        {
            // Arrange
            var error = "error";
            var errorDescription = "error description";
            var networkId = Guid.NewGuid();
            var clientId = Guid.NewGuid().ToString();
            var httpClient = _service.CreateDefaultClient();

            _service.NetworkPerspectiveCoreMock
                .Setup(x => x.ValidateTokenAsync(It.IsAny<SecureString>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new TokenValidationResponse(networkId, Guid.NewGuid()));

            _service.SecretRepositoryMock
                .Setup(x => x.GetSecretAsync(MicrosoftKeys.MicrosoftClientIdKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(clientId.ToSecureString());

            var networkConfig = new NetworkConfigDto
            {
                SyncMsTeams = false
            };

            var networkCreateResponse = await new NetworksClient(httpClient)
                .NetworksPostAsync(networkConfig);

            var authClient = new AuthClient(httpClient);

            var authResponse = await authClient
                .AuthAsync(null);

            // Act
            Func<Task> func = async () => await authClient.CallbackAsync(null, null, error, errorDescription);

            // Assert
            await func.Should()
                .ThrowAsync<Office365ClientException>()
                .Where(x => JsonConvert.DeserializeObject<ProblemDetails>(x.Response).Detail.Contains(error));
        }
    }
}