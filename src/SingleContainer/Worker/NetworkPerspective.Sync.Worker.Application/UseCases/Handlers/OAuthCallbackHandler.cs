using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Utils.CQS.Queries;
using NetworkPerspective.Sync.Worker.Application.Domain.OAuth;
using NetworkPerspective.Sync.Worker.Application.Exceptions;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application.UseCases.Handlers;

internal class OAuthCallbackHandler(IMemoryCache cache, IOAuthService oAuthService, ILogger<OAuthCallbackHandler> logger) : IRequestHandler<HandleOAuthCallbackRequest, AckDto>
{
    public async Task<AckDto> HandleAsync(HandleOAuthCallbackRequest dto, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Handling OAuth callback");

        if (!cache.TryGetValue(dto.State, out OAuthContext context))
            throw new OAuthException("State does not match initialized value");

        await oAuthService.HandleAuthorizationCodeCallbackAsync(dto.Code, context, stoppingToken);

        return new AckDto { CorrelationId = dto.CorrelationId };
    }
}