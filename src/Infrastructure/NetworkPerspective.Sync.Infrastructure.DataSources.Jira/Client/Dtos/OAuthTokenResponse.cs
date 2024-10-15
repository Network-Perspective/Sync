using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Dtos;

public class OAuthTokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
}