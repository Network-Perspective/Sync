using System;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Configs
{
    internal class AuthConfig
    {
        public string[] Scopes { get; set; } = Array.Empty<string>();
        public string[] UserScopes { get; set; } = Array.Empty<string>();
    }
}