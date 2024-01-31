using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using Moq;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;

using Newtonsoft.Json;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Client.HttpClients
{
    public class AuthTokenHandlerTests
    {
        const string Pattern = "token-pattern-{0}";


        private readonly HttpResponseMessage _successMessage = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(JsonConvert.SerializeObject(new SampleResponseWithError { IsOk = true }), Encoding.UTF8)
        };

        private readonly HttpResponseMessage _tokenRevokedMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized)
        {
            Content = new StringContent(JsonConvert.SerializeObject(new SampleResponseWithError { IsOk = false, Error = SlackApiErrorCodes.TokenRevoked }), Encoding.UTF8)
        };

        private readonly HttpResponseMessage _notExpectedMessage = new HttpResponseMessage(HttpStatusCode.Accepted)
        {
            Content = new StringContent("noone expects the spanish inquisition", Encoding.UTF8)
        };

        [Fact]
        public async Task ShouldSetAuthorizationHeaderFromSecretRepository()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var networkIdProviderMock = new Mock<INetworkIdProvider>();
            networkIdProviderMock
                .Setup(x => x.Get())
                .Returns(networkId);

            var token = Guid.NewGuid().ToString();
            var secretRepositoryMock = new Mock<ICachedSecretRepository>();
            secretRepositoryMock
                .Setup(x => x.GetSecretAsync(string.Format(Pattern, networkId.ToString()), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token.ToSecureString());

            var handler = new AuthTokenHandler(networkIdProviderMock.Object, secretRepositoryMock.Object, Pattern, NullLogger<AuthTokenHandler>.Instance)
            {
                InnerHandler = new TestHandler(_successMessage)
            };
            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://networkperspective.io/");

            // Act
            var result = await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            result.RequestMessage.Headers.Authorization.Scheme.Should().Be("Bearer");
            result.RequestMessage.Headers.Authorization.Parameter.Should().Be(token);
        }

        [Fact]
        public async Task ShouldRetakeTokenFromRepositoryOnTokenRevoked()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var networkIdProviderMock = new Mock<INetworkIdProvider>();
            networkIdProviderMock
                .Setup(x => x.Get())
                .Returns(networkId);

            var token1 = Guid.NewGuid().ToString();
            var token2 = Guid.NewGuid().ToString();
            var secretRepositoryMock = new Mock<ICachedSecretRepository>();
            secretRepositoryMock
                .SetupSequence(x => x.GetSecretAsync(string.Format(Pattern, networkId.ToString()), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token1.ToSecureString())
                .ReturnsAsync(token2.ToSecureString());

            var handler = new AuthTokenHandler(networkIdProviderMock.Object, secretRepositoryMock.Object, Pattern, NullLogger<AuthTokenHandler>.Instance)
            {
                InnerHandler = new TestHandler(_tokenRevokedMessage, _successMessage)
            };
            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://networkperspective.io/");

            // Act
            var result = await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            result.RequestMessage.Headers.Authorization.Scheme.Should().Be("Bearer");
            result.RequestMessage.Headers.Authorization.Parameter.Should().Be(token2);
        }

        [Fact]
        public async Task ShouldReturnResponseOnNotExpectedPayload()
        {
            // Arrange
            var networkId = Guid.NewGuid();
            var networkIdProviderMock = new Mock<INetworkIdProvider>();
            networkIdProviderMock
                .Setup(x => x.Get())
                .Returns(networkId);

            var token = Guid.NewGuid().ToString();
            var secretRepositoryMock = new Mock<ICachedSecretRepository>();
            secretRepositoryMock
                .Setup(x => x.GetSecretAsync(string.Format(Pattern, networkId.ToString()), It.IsAny<CancellationToken>()))
                .ReturnsAsync(token.ToSecureString());

            var handler = new AuthTokenHandler(networkIdProviderMock.Object, secretRepositoryMock.Object, Pattern, NullLogger<AuthTokenHandler>.Instance)
            {
                InnerHandler = new TestHandler(_notExpectedMessage)
            };
            var invoker = new HttpMessageInvoker(handler);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://networkperspective.io/");

            // Act
            var result = await invoker.SendAsync(request, CancellationToken.None);

            // Assert
            var payload = await result.Content.ReadAsStringAsync();
            payload.Should().Be("noone expects the spanish inquisition");
        }
    }

    public class TestHandler : DelegatingHandler
    {
        private readonly Queue<HttpResponseMessage> _responseSequence;

        public TestHandler(params HttpResponseMessage[] responseSequence)
        {
            _responseSequence = new Queue<HttpResponseMessage>(responseSequence);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = _responseSequence.Dequeue();
            response.RequestMessage = request;
            return Task.FromResult(response);
        }
    }
}