using System.Net.Http;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Slack.Configs;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients
{
    internal interface ISlackHttpClientFactory
    {
        ISlackHttpClient CreateWithBotToken();
        ISlackHttpClient CreateWithUserToken();
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

        public ISlackHttpClient CreateWithBotToken()
            => CreateUsingClientName(Consts.SlackApiHttpClientWithBotTokenName);

        public ISlackHttpClient CreateWithUserToken()
            => CreateUsingClientName(Consts.SlackApiHttpClientWithUserTokenName);

        public ISlackHttpClient Create()
            => CreateUsingClientName(Consts.SlackApiHttpClientName);

        private ISlackHttpClient CreateUsingClientName(string clientName)
        {
            var httpClient = _httpClientFactory.CreateClient(clientName);

            var slackHttpClient = new SlackHttpClient(httpClient, _loggerFactory.CreateLogger<SlackHttpClient>());
            return new ResilientSlackHttpClientDecorator(slackHttpClient, _options, _loggerFactory.CreateLogger<ResilientSlackHttpClientDecorator>());
        }
    }
}