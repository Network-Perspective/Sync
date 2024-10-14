using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.ApiClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.HttpClients;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client;

public interface IJiraUnauthorizedFacade
{
    Task<OAuthTokenResponse> RefreshTokenFlowAsync(string clientId, string clientSecret, string refreshToken, CancellationToken stoppingToken = default);
}

internal class JiraUnauthorizedFacade(IJiraHttpClient jiraHttpClient) : IJiraUnauthorizedFacade
{
    private readonly OAuthClient _oauthClient = new(jiraHttpClient);

    public Task<OAuthTokenResponse> RefreshTokenFlowAsync(string clientId, string clientSecret, string refreshToken, CancellationToken stoppingToken = default)
        => _oauthClient.RefreshTokenFlowAsync(clientId, clientSecret, refreshToken, stoppingToken);
}