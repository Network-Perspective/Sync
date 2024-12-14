using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.CQS.Queries;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Worker.Application.UseCases.Handlers;

internal class SetSecretsHandler(IVault vault, ILogger<SetSecretsHandler> logger) : IRequestHandler<SetSecretsRequest, AckDto>
{
    public async Task<AckDto> HandleAsync(SetSecretsRequest request, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Setting {count} secrets", request.Secrets.Count);

        foreach (var secret in request.Secrets)
            await vault.SetSecretAsync(secret.Key, secret.Value.ToSecureString(), stoppingToken);

        logger.LogInformation("Secrets has been set");

        return new AckDto { CorrelationId = request.CorrelationId };
    }
}
