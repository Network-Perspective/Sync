using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.ApiClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client;

public interface ISlackClientUnauthorizedFacade : IDisposable
{
    Task<OAuthAccessResponse> AccessAsync(OAuthAccessRequest request, CancellationToken stoppingToken = default);
    Task<OAuthExchangeResponse> ExchangeLegacyTokenAsync(OAuthExchangeRequest request, CancellationToken stoppingToken = default);
    Task<TestResponse> TestTokenAsync(SecureString token, CancellationToken stoppingToken = default);
}

internal class SlackClientUnauthorizedFacade : ISlackClientUnauthorizedFacade
{
    private readonly ISlackHttpClient _slackHttpClient;
    private readonly OAuthClient _oauthClient;
    private readonly AuthClient _authClient;

    public SlackClientUnauthorizedFacade(ISlackHttpClient slackHttpClient)
    {
        _slackHttpClient = slackHttpClient;
        _oauthClient = new OAuthClient(_slackHttpClient);
        _authClient = new AuthClient(_slackHttpClient);
    }

    public void Dispose()
    {
        _slackHttpClient?.Dispose();
    }

    public Task<OAuthAccessResponse> AccessAsync(OAuthAccessRequest request, CancellationToken stoppingToken = default)
        => _oauthClient.AccessAsync(request, stoppingToken);

    public Task<OAuthExchangeResponse> ExchangeLegacyTokenAsync(OAuthExchangeRequest request, CancellationToken stoppingToken = default)
        => _oauthClient.ExchangeLegacyTokenAsync(request, stoppingToken);

    public Task<TestResponse> TestTokenAsync(SecureString token, CancellationToken stoppingToken = default)
        => _authClient.TestAsync(token, stoppingToken);
}