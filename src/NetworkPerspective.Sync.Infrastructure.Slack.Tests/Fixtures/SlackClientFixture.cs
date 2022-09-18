using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Infrastructure.SecretStorage;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Fixtures
{
    public class SlackClientFixture : IDisposable
    {
        public HttpClient HttpClient { get; }
        public IHttpClientFactory HttpClientFactory { get; }

        public SlackClientFixture()
        {
            var secretStorageOptions = Options.Create(new AzureKeyVaultConfig { BaseUrl = "https://nptestvault.vault.azure.net/" });

            var secretRepository = new InternalAzureKeyVaultClient(TokenCredentialFactory.Create(), secretStorageOptions, NullLogger<InternalAzureKeyVaultClient>.Instance);
            var slackToken = secretRepository.GetSecretAsync(string.Format(SlackKeys.TokenKeyPattern, "unit-test")).Result;

            HttpClient = new HttpClient
            {
                BaseAddress = new Uri("https://slack.com/api/")
            };
            HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", new NetworkCredential(string.Empty, slackToken).Password);

            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(HttpClient);

            HttpClientFactory = httpClientFactoryMock.Object;
        }

        public void Dispose()
            => HttpClient?.Dispose();
    }
}