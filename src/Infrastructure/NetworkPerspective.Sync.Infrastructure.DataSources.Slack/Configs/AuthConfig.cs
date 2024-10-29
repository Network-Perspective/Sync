namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Configs;

internal class AuthConfig
{
    public string[] Scopes { get; set; } = [];
    public string[] UserScopes { get; set; } = [];
    public string[] AdminUserScopes { get; set; } = [];
}