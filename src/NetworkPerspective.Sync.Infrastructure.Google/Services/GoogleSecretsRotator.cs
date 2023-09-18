using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services;

public class GoogleSecretsRotator : ISecretRotator
{
    private readonly ILogger<GoogleSecretsRotator> _logger;

    public GoogleSecretsRotator(ILogger<GoogleSecretsRotator> logger)
    {
        _logger = logger;
    }
    
    public void RotateSecrets()
    {
        _logger.LogInformation("Rotating Google secrets");
    }
}