using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Utils.CQS.Queries;
using NetworkPerspective.Sync.Worker.Application.Domain.OAuth;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application.UseCases.Handlers;

internal class InitializeOAuthHandler(IOAuthService oAuthService, IConnectorContextAccessor connectorContextAccessor, ILogger<InitializeOAuthHandler> logger) : IQueryHandler<InitializeOAuthRequest, InitializeOAuthResponse>
{
    public async Task<InitializeOAuthResponse> HandleAsync(InitializeOAuthRequest dto, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Initializing OAuth for connector '{connectorId}' (of type '{connectorType}')", dto.Connector.Id, dto.Connector.Type);

        var context = new OAuthContext(connectorContextAccessor.Context, dto.CallbackUri);
        var result = await oAuthService.InitializeOAuthAsync(context, stoppingToken);

        var response = new InitializeOAuthResponse
        {
            CorrelationId = dto.CorrelationId,
            AuthUri = result.AuthUri,
            State = result.State,
            StateExpirationTimestamp = result.StateExpirationTimestamp
        };

        return response;
    }
}
