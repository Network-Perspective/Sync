using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Utils.CQS.Middlewares;
using NetworkPerspective.Sync.Utils.CQS.PreProcessors;
using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.CQS;

public interface IMediator
{
    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken stoppingToken = default)
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse;
}

internal class Mediator(IServiceProvider serviceProvider) : IMediator
{
    async Task<TResponse> IMediator.SendAsync<TQuery, TResponse>(TQuery request, CancellationToken stoppingToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();

        var preprocessors = scope.ServiceProvider.GetRequiredService<IEnumerable<IPreProcessor>>();

        foreach (var preprocessor in preprocessors.Reverse())
            await preprocessor.PreprocessAsync<TQuery, TResponse>(request, scope, stoppingToken);

        QueryHandlerDelegate<TQuery, TResponse> pipeline = (query, ct) =>
        {
            var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TQuery, TResponse>>();
            return handler.HandleAsync(query, ct);
        };

        var middlewares = scope.ServiceProvider.GetRequiredService<IEnumerable<IMediatorMiddleware>>();

        foreach (var middleware in middlewares.Reverse())
        {
            var next = pipeline;
            pipeline = (req, token) => middleware.HandleQueryAsync(req, next, token);
        }

        return await pipeline(request, stoppingToken);
    }
}