using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Utils.CQS.Commands;
using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.CQS.Middlewares;

public class LoggingMiddleware(ILogger<LoggingMiddleware> logger) : IMediatorMiddleware
{
    async Task IMediatorMiddleware.HandleCommandAsync<TCommand>(TCommand request, CommandHandlerDelegate<TCommand> next, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Handling command '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TCommand).Name);
            await next(request, cancellationToken);
            logger.LogInformation("Finished handling command '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TCommand).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception has been thrown during execution of command '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TCommand).Name);
            throw;
        }
    }

    async Task<TResponse> IMediatorMiddleware.HandleQueryAsync<TQuery, TResponse>(TQuery request, QueryHandlerDelegate<TQuery, TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Handling query '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TQuery).Name);
            var response = await next(request, cancellationToken);
            logger.LogInformation("Finished handling query '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TQuery).Name);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception has been thrown during execution of query '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TQuery).Name);
            throw;
        }
    }
}
