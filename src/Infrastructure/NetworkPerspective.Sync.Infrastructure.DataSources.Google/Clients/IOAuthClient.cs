using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Model;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Clients;

internal interface IOAuthClient
{
    Task<TokenResponse> ExchangeCodeForTokenAsync(string code, string clientId, string clientSecret, string callbackUri, CancellationToken stoppingToken = default);
    Task<TokenResponse> RefreshTokenAsync(string refreshToken, string clientId, string clientSecret, CancellationToken stoppingToken = default);
}

internal class OAuthClient : IOAuthClient
{
    public async Task<TokenResponse> ExchangeCodeForTokenAsync(string code, string clientId, string clientSecret, string callbackUri, CancellationToken stoppingToken = default)
    {
        var initializer = new AuthorizationCodeFlow.Initializer(GoogleAuthConsts.OidcAuthorizationUrl, GoogleAuthConsts.OidcTokenUrl)
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            }
        };
        var codeFlow = new AuthorizationCodeFlow(initializer);
        var response = await codeFlow.ExchangeCodeForTokenAsync(string.Empty, code, callbackUri, stoppingToken);

        return new TokenResponse(response.AccessToken, response.RefreshToken);
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, string clientId, string clientSecret, CancellationToken stoppingToken = default)
    {
        var initializer = new AuthorizationCodeFlow.Initializer(GoogleAuthConsts.OidcAuthorizationUrl, GoogleAuthConsts.OidcTokenUrl)
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = clientId,
                ClientSecret = clientSecret
            }
        };
        var codeFlow = new AuthorizationCodeFlow(initializer);
        var response = await codeFlow.RefreshTokenAsync(string.Empty, refreshToken, stoppingToken);

        return new TokenResponse(response.AccessToken, response.RefreshToken);
    }
}