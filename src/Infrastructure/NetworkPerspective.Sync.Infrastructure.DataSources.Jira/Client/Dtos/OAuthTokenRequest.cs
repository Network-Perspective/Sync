using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Dtos;

internal class OAuthTokenRequest
{
    [JsonProperty("grant_type")]
    public string GrantType { get; set; }

    [JsonProperty("client_id")]
    public string ClientId { get; set; }

    [JsonProperty("client_secret")]
    public string ClientSecret { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
}