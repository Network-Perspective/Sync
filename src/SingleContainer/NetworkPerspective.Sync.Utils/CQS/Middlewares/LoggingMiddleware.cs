using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.CQS.Middlewares;

public class LoggingMiddleware(ILogger<LoggingMiddleware> logger) : IMediatorMiddleware
{
    async Task<TResponse> IMediatorMiddleware.HandleQueryAsync<TRequest, TResponse>(TRequest request, QueryHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Handling request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
            var response = await next(request, cancellationToken);
            logger.LogInformation("Finished handling request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception has been thrown during execution of request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
            throw;
        }
    }
}