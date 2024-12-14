using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.CQS.Commands;
using NetworkPerspective.Sync.Utils.CQS.Middlewares;
using NetworkPerspective.Sync.Utils.CQS.Queries;
using NetworkPerspective.Sync.Worker.Application.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application.UseCases.Middlewares;

internal class ConnectorScopedLoggingMiddleware : IMediatorMiddleware
{
    private readonly IStatusLogger _statusLogger;

    public ConnectorScopedLoggingMiddleware(IStatusLogger statusLogger)
    {
        _statusLogger = statusLogger;
    }

    Task IMediatorMiddleware.HandleCommandAsync<TCommand>(TCommand command, CommandHandlerDelegate<TCommand> next, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    async Task<TResponse> IMediatorMiddleware.HandleQueryAsync<TQuery, TResponse>(TQuery query, QueryHandlerDelegate<TQuery, TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            await _statusLogger.LogInfoAsync($"Running action '{query.UserFriendlyName}'", cancellationToken);
            var result = await next(query, cancellationToken);
            await _statusLogger.LogInfoAsync($"Action completed '{query.UserFriendlyName}'", cancellationToken);
            return result;
        }
        catch (Exception)
        {
            await _statusLogger.LogWarningAsync($"Unable to perform action '{query.UserFriendlyName}'", cancellationToken);
            throw;
        }
    }
}
