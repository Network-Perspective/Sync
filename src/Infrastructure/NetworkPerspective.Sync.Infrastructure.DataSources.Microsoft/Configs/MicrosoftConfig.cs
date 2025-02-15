namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Configs;

internal class MicrosoftConfig
{
    public AuthConfig Auth { get; set; } = new();
    public ResiliencyConfig Resiliency { get; set; } = new();
}
