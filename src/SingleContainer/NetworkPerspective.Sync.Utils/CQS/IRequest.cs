using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.Utils.CQS;

public interface IRequest
{
    Guid CorrelationId { get; set; }
}

public interface IRequest<out TResponse> : IRequest
    where TResponse : class
{
}

public interface IResponse
{
    Guid CorrelationId { get; set; }
}

// -----------------------


public interface IRequestHandler<in TRequest>
    where TRequest : class, IRequest
{
    Task HandleAsync(TRequest request, CancellationToken stoppingToken = default);
}

public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
    where TResponse : class, IResponse
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken stoppingToken = default);
}

// -----------------------

public delegate Task CommandHandler<TRequest>(TRequest request, CancellationToken stoppingToken)
    where TRequest : class, IRequest;

public delegate Task<TResponse> QueryHandler<TRequest, TResponse>(TRequest request, CancellationToken stoppingToken)
    where TRequest : class, IRequest
    where TResponse : class, IResponse;

public interface IMediatorMiddleware
{
    Task HandleAsync<TRequest>(TRequest request, CommandHandler<TRequest> next, CancellationToken cancellationToken)
        where TRequest : class, IRequest;

    Task<TResponse> HandleAsync<TRequest, TResponse>(TRequest request, QueryHandler<TRequest, TResponse> next, CancellationToken cancellationToken)
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse;
}

public class LoggingMiddleware(ILogger<LoggingMiddleware> logger) : IMediatorMiddleware
{
    public async Task HandleAsync<TRequest>(TRequest request, CommandHandler<TRequest> next, CancellationToken cancellationToken)
        where TRequest : class, IRequest
    {
        logger.LogInformation("Handling request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
        await next(request, cancellationToken);
        logger.LogInformation("Finished handling request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
    }

    public async Task<TResponse> HandleAsync<TRequest, TResponse>(TRequest request, QueryHandler<TRequest, TResponse> next, CancellationToken cancellationToken)
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse
    {
        logger.LogInformation("Handling request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
        var response = await next(request, cancellationToken); // Call the next middleware or handler
        logger.LogInformation("Finished handling request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
        return response;
    }
}

// -----------------------

public interface IMediator
{
    Task SendAsync<TRequest>(TRequest request, CancellationToken stoppingToken = default)
        where TRequest : class, IRequest;

    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken stoppingToken = default)
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse;
}

internal class Mediator(IServiceProvider serviceProvider, IEnumerable<IMediatorMiddleware> middlewares) : IMediator
{

    async Task IMediator.SendAsync<TRequest>(TRequest request, CancellationToken stoppingToken)
    {
        CommandHandler<TRequest> pipeline = HandleCommandAsync;

        foreach (var middleware in middlewares.Reverse())
        {
            var next = pipeline;
            pipeline = (req, token) => middleware.HandleAsync(req, next, token);
        }

        await pipeline(request, stoppingToken);
    }

    async Task<TResponse> IMediator.SendAsync<TRequest, TResponse>(TRequest request, CancellationToken stoppingToken)
    {
        QueryHandler<TRequest, TResponse> pipeline = HandleRequestAsync<TRequest, TResponse>;

        foreach (var middleware in middlewares.Reverse())
        {
            var next = pipeline;
            pipeline = (req, token) => middleware.HandleAsync(req, next, token);
        }

        return await pipeline(request, stoppingToken);
    }

    private async Task HandleCommandAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : class, IRequest
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        await handler.HandleAsync(request, cancellationToken);
    }

    private async Task<TResponse> HandleRequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        return await handler.HandleAsync(request, cancellationToken);
    }
}

public static class ServiceCollectionExctensions
{
    public static CqsBuilder AddCqs(this IServiceCollection services)
    {
        services.AddSingleton<IMediator, Mediator>();

        return new CqsBuilder(services);
    }
}

public class CqsBuilder(IServiceCollection services)
{
    public CqsBuilder AddHandler<THandler, TRequest>()
        where THandler : class, IRequestHandler<TRequest>
        where TRequest : class, IRequest
    {
        services.AddScoped<IRequestHandler<TRequest>, THandler>();
        return this;
    }

    public CqsBuilder AddHandler<THandler, TRequest, TResponse>()
        where THandler : class, IRequestHandler<TRequest, TResponse>
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse
    {
        services.AddScoped<IRequestHandler<TRequest, TResponse>, THandler>();
        return this;
    }

    public CqsBuilder AddMiddleware<TMiddleware>()
        where TMiddleware : class, IMediatorMiddleware
    {
        services.AddSingleton<TMiddleware, TMiddleware>();
        return this;
    }

    public CqsBuilder AddMiddleware(IMediatorMiddleware middleware)
    {
        services.AddSingleton(middleware);
        return this;
    }

}