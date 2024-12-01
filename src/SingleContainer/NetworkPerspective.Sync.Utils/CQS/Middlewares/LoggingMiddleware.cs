using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Utils.CQS.Commands;
using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.CQS.Middlewares;

public class LoggingMiddleware(ILogger<LoggingMiddleware> logger) : IMediatorMiddleware
{
    public async Task HandleCommandAsync<TRequest>(TRequest request, CommandHandlerDelegate<TRequest> next, CancellationToken cancellationToken)
        where TRequest : class, ICommand
    {
        logger.LogInformation("Handling request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
        await next(request, cancellationToken);
        logger.LogInformation("Finished handling request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
    }

    public async Task<TResponse> HandleQueryAsync<TRequest, TResponse>(TRequest request, QueryHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
        where TRequest : class, IQuery<TResponse>
        where TResponse : class, IResponse
    {
        logger.LogInformation("Handling request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
        var response = await next(request, cancellationToken); // Call the next middleware or handler
        logger.LogInformation("Finished handling request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
        return response;
    }
}
