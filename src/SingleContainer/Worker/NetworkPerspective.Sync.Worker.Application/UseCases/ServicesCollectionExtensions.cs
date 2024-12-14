using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Utils.CQS;
using NetworkPerspective.Sync.Utils.CQS.Middlewares;
using NetworkPerspective.Sync.Worker.Application.UseCases.Handlers;
using NetworkPerspective.Sync.Worker.Application.UseCases.Middlewares;
using NetworkPerspective.Sync.Worker.Application.UseCases.Preprocessors;

namespace NetworkPerspective.Sync.Worker.Application.UseCases;

internal static class ServicesCollectionExtensions
{
    public static IServiceCollection AddUseCasesHandling(this IServiceCollection services)
    {
        services
            .AddCqs()
            .AddPreProcessor<ConnectorScopedPreProcessor>()
            .AddMiddleware<LoggingMiddleware>()
            .AddMiddleware<ConnectorScopedLoggingMiddleware>()
            .AddHandler<ConnectorStatusHandler, ConnectorStatusRequest, ConnectorStatusResponse>()
            .AddHandler<WorkerCapabilitiesHandler, WorkerCapabilitiesRequest, WorkerCapabilitiesResponse>()
            .AddHandler<InitializeOAuthHandler, InitializeOAuthRequest, InitializeOAuthResponse>()
            .AddHandler<OAuthCallbackHandler, HandleOAuthCallbackRequest, AckDto>()
            .AddHandler<RotateSecretsHandler, RotateSecretsRequest, AckDto>()
            .AddHandler<SetSecretsHandler, SetSecretsRequest, AckDto>()
            .AddHandler<SyncHandler, SyncRequest, SyncResponse>();

        return services;
    }
}
