using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Infrastructure.Slack.Configs;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients
{
    internal interface ISlackHttpClientFactory
    {
        ISlackHttpClient CreateWithToken(SecureString token);
        ISlackHttpClient Create();
    }

    internal class SlackHttpClientFactory : ISlackHttpClientFactory
    {
        private readonly Resiliency _options;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IHttpClientFactory _httpClientFactory;

        public SlackHttpClientFactory(IOptions<Resiliency> options, ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory)
        {
            _options = options.Value;
            _loggerFactory = loggerFactory;
            _httpClientFactory = httpClientFactory;
        }

        public ISlackHttpClient CreateWithToken(SecureString token)
            => Create(token);

        public ISlackHttpClient Create()
            => Create(null);

        private ISlackHttpClient Create(SecureString token)
        {
            var httpClient = _httpClientFactory.CreateClient(Consts.SlackApiHttpClientName);

            if(token is not null)
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.ToSystemString());

            var slackHttpClient = new SlackHttpClient(httpClient, _loggerFactory.CreateLogger<SlackHttpClient>());
            return new ResilientSlackHttpClientDecorator(slackHttpClient, _options, _loggerFactory.CreateLogger<ResilientSlackHttpClientDecorator>());
        }
    }
}