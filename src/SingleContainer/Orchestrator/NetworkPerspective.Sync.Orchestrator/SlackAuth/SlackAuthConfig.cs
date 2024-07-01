using System;

namespace NetworkPerspective.Sync.Orchestrator.SlackAuth
{
    internal class SlackAuthConfig
    {
        public string[] Scopes { get; set; } = Array.Empty<string>();
        public string[] UserScopes { get; set; } = Array.Empty<string>();
        public string[] AdminUserScopes { get; set; } = Array.Empty<string>();
    }
}