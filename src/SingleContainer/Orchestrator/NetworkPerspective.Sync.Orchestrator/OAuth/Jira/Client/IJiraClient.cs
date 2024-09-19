using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Orchestrator.OAuth.Jira.Client.Model;
using NetworkPerspective.Sync.Utils.Extensions;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Orchestrator.OAuth.Jira.Client;

public interface IJiraClient
{
    Task<TokenResponse> ExchangeCodeForTokenAsync(string code, Uri callbackUri, CancellationToken stoppingToken = default);
}

internal class JiraClient(IVault vault, IOptions<JiraConfig> config) : IJiraClient
{

    public async Task<TokenResponse> ExchangeCodeForTokenAsync(string code, Uri callbackUri, CancellationToken stoppingToken = default)
    {
        var clientId = await vault.GetSecretAsync(JiraKeys.JiraClientIdKey, stoppingToken);
        var clientSecret = await vault.GetSecretAsync(JiraKeys.JiraClientSecretKey, stoppingToken);

        using var httpclient = new HttpClient
        {
            BaseAddress = new Uri(config.Value.BaseUrl)
        };

        var requestObject = new TokenRequest
        {
            GrantType = "authorization_code",
            ClientId = clientId.ToSystemString(),
            ClientSecret = clientSecret.ToSystemString(),
            Code = code,
            RedirectUri = callbackUri.ToString()
        };
        var requestPayload = JsonConvert.SerializeObject(requestObject);
        var requestContent = new StringContent(requestPayload, Encoding.UTF8, "application/json");
        var response = await httpclient.PostAsync("oauth/token", requestContent, stoppingToken);

        var responsePayload = await response.Content.ReadAsStringAsync(stoppingToken);
        var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responsePayload);
        return tokenResponse;
    }
}