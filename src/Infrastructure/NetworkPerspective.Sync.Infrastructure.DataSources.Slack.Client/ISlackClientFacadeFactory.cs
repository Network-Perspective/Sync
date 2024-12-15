using System.Net.Http;
using System.Threading;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Configs;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Pagination;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;

public interface ISlackClientFacadeFactory
{
    ISlackClientBotScopeFacade CreateWithBotToken(CancellationToken stoppingToken = default);
    ISlackClientUserScopeFacade CreateWithUserToken(CancellationToken stoppingToken = default);
    ISlackClientUnauthorizedFacade CreateUnauthorized();
}

internal class SlackClientFacadeFactory(IOptions<Resiliency> options, ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory, CursorPaginationHandler cursorPaginationHandler) : ISlackClientFacadeFactory
{
    public ISlackClientBotScopeFacade CreateWithBotToken(CancellationToken stoppingToken = default)
    {
        var slackClient = CreateUsingClientName(Consts.SlackApiHttpClientWithBotTokenName);

        return new SlackClientBotScopeFacade(slackClient, cursorPaginationHandler);
    }

    public ISlackClientUserScopeFacade CreateWithUserToken(CancellationToken stoppingToken = default)
    {
        var slackClient = CreateUsingClientName(Consts.SlackApiHttpClientWithUserTokenName);

        return new SlackClientUserScopeFacade(slackClient, cursorPaginationHandler);
    }

    public ISlackClientUnauthorizedFacade CreateUnauthorized()
    {
        var slackHttpClient = CreateUsingClientName(Consts.SlackApiHttpClientName);

        return new SlackClientUnauthorizedFacade(slackHttpClient);
    }

    private ResilientSlackHttpClientDecorator CreateUsingClientName(string clientName)
    {
        var httpClient = httpClientFactory.CreateClient(clientName);

        var slackHttpClient = new SlackHttpClient(httpClient, loggerFactory.CreateLogger<SlackHttpClient>());
        return new ResilientSlackHttpClientDecorator(slackHttpClient, options.Value, loggerFactory.CreateLogger<ResilientSlackHttpClientDecorator>());
    }
}