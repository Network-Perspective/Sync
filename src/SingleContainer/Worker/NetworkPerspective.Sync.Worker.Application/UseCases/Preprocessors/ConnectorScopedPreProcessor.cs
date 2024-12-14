using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Utils.CQS.PreProcessors;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application.UseCases.Preprocessors;

internal class ConnectorScopedPreProcessor(IConnectorContextAccessor connectorContextProvider) : IPreProcessor
{
    Task IPreProcessor.PreprocessAsync<TCommand>(TCommand command, IServiceScope scope, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    Task IPreProcessor.PreprocessAsync<TQuery, TResponse>(TQuery request, IServiceScope scope, CancellationToken cancellationToken)
    {
        if (request is IConnectorScoped scopedRequest)
        {
            var connector = scopedRequest.Connector;
            connectorContextProvider.Context = new ConnectorContext(connector.Id, connector.Type, connector.Properties);
        }

        return Task.CompletedTask;
    }
}
