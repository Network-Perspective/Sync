using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Orchestrator.OAuth.Jira.Model;

public class TokenResponse
{
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
}