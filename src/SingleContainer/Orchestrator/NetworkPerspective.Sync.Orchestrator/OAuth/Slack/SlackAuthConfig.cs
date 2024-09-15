using System;

namespace NetworkPerspective.Sync.Orchestrator.OAuth.Slack;

internal class SlackAuthConfig
{
    public string[] Scopes { get; set; } = [];
    public string[] UserScopes { get; set; } = [];
    public string[] AdminUserScopes { get; set; } = [];
}