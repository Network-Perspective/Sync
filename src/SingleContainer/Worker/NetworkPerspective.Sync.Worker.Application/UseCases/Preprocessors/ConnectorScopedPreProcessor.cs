using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Utils.CQS.PreProcessors;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.OAuth;
using NetworkPerspective.Sync.Worker.Application.Exceptions;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application.UseCases.Preprocessors;

internal class ConnectorScopedPreProcessor(IConnectorContextAccessor connectorContextProvider, IMemoryCache cache) : IPreProcessor
{
    Task IPreProcessor.PreprocessAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
    {
        if (request is IConnectorScoped scopedRequest)
        {
            var connector = scopedRequest.Connector;
            connectorContextProvider.Context = new ConnectorContext(connector.Id, connector.Type, connector.Properties);
        }
        else if (request is HandleOAuthCallbackRequest handleOAuthRequest)
        {
            if (!cache.TryGetValue(handleOAuthRequest.State, out OAuthContext context))
                throw new OAuthException("State does not match initialized value");

            connectorContextProvider.Context = context.Connector;
        }

        return Task.CompletedTask;
    }
}