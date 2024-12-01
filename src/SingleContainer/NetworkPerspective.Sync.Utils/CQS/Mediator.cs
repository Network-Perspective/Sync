using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Utils.CQS.Commands;
using NetworkPerspective.Sync.Utils.CQS.Middlewares;
using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.CQS;

internal class Mediator(IServiceProvider serviceProvider, IEnumerable<IMediatorMiddleware> middlewares) : IMediator
{
    async Task IMediator.SendCommandAsync<TCommand>(TCommand request, CancellationToken stoppingToken)
    {
        CommandHandlerDelegate<TCommand> pipeline = HandleCommandAsync;

        foreach (var middleware in middlewares.Reverse())
        {
            var next = pipeline;
            pipeline = (req, token) => middleware.HandleCommandAsync(req, next, token);
        }

        await pipeline(request, stoppingToken);
    }

    async Task<TResponse> IMediator.SendQueryAsync<TQuery, TResponse>(TQuery request, CancellationToken stoppingToken)
    {
        QueryHandlerDelegate<TQuery, TResponse> pipeline = HandleQueryAsync<TQuery, TResponse>;

        foreach (var middleware in middlewares.Reverse())
        {
            var next = pipeline;
            pipeline = (req, token) => middleware.HandleQueryAsync(req, next, token);
        }

        return await pipeline(request, stoppingToken);
    }

    private async Task HandleCommandAsync<TCommand>(TCommand request, CancellationToken cancellationToken)
        where TCommand : class, ICommand
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        await handler.HandleAsync(request, cancellationToken);
    }

    private async Task<TResponse> HandleQueryAsync<TQuery, TResponse>(TQuery request, CancellationToken cancellationToken)
        where TQuery : class, IQuery<TResponse>
        where TResponse : class, IResponse
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();
        return await handler.HandleAsync(request, cancellationToken);
    }
}
