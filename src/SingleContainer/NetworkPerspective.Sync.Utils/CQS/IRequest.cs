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

public interface IRequest<out TResponse> : IRequest where TResponse : class
{
}

public interface IResponse
{
    Guid CorrelationId { get; set; }
}

// -----------------------


public interface IRequestHandler<TRequest>
    where TRequest : class, IRequest
{
    Task HandleAsync(TRequest request, CancellationToken stoppingToken = default);
}

public interface IRequestHandler<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
    where TResponse : class, IResponse
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken stoppingToken = default);
}

// -----------------------

public interface IMediatorMiddleware
{
    Task HandleAsync<TRequest>(
        TRequest request,
        Func<TRequest, CancellationToken, Task> next,
        CancellationToken cancellationToken)
        where TRequest : class, IRequest;

    Task<TResponse> HandleAsync<TRequest, TResponse>(
        TRequest request,
        Func<TRequest, CancellationToken, Task<TResponse>> next,
        CancellationToken cancellationToken)
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse;
}

public class LoggingMiddleware : IMediatorMiddleware
{
    private readonly ILogger<LoggingMiddleware> _logger;

    public LoggingMiddleware(ILogger<LoggingMiddleware> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync<TRequest>(
        TRequest request,
        Func<TRequest, CancellationToken, Task> next,
        CancellationToken cancellationToken)
        where TRequest : class, IRequest
    {
        _logger.LogInformation("Handling request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
        await next(request, cancellationToken);
        _logger.LogInformation("Finished handling request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
    }

    public async Task<TResponse> HandleAsync<TRequest, TResponse>(
        TRequest request,
        Func<TRequest, CancellationToken, Task<TResponse>> next,
        CancellationToken cancellationToken)
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse
    {
        _logger.LogInformation("Handling request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
        var response = await next(request, cancellationToken); // Call the next middleware or handler
        _logger.LogInformation("Finished handling request '{CorrelationId}' of type {Type}", request.CorrelationId, typeof(TRequest).Name);
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

internal class Mediator(IServiceProvider serviceProvider, IEnumerable<IMediatorMiddleware> _middlewares) : IMediator
{

    public async Task SendAsync<TRequest>(TRequest request, CancellationToken stoppingToken)
        where TRequest : class, IRequest
    {
        // Build the middleware pipeline
        Func<TRequest, CancellationToken, Task> pipeline = HandleCommandAsync;

        foreach (var middleware in _middlewares.Reverse())
        {
            var next = pipeline;
            pipeline = (req, token) => middleware.HandleAsync(req, next, token);
        }

        // Execute the pipeline
        await pipeline(request, stoppingToken);
    }

    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken stoppingToken)
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse
    {
        // Build the middleware pipeline
        Func<TRequest, CancellationToken, Task<TResponse>> pipeline = (req, token) => HandleRequestAsync<TRequest, TResponse>(req, token);

        foreach (var middleware in _middlewares.Reverse())
        {
            var next = pipeline;
            pipeline = (req, token) => middleware.HandleAsync(req, next, token);
        }

        // Execute the pipeline
        return await pipeline(request, stoppingToken);
    }

    private async Task HandleCommandAsync<TRequest>(TRequest request, CancellationToken cancellationToken)
        where TRequest : class, IRequest
    {
        // Resolve the appropriate handler from the service provider
        await using var scope = serviceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        await handler.HandleAsync(request, cancellationToken);
    }

    private async Task<TResponse> HandleRequestAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse
    {
        // Resolve the appropriate handler from the service provider
        await using var scope = serviceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        return await handler.HandleAsync(request, cancellationToken);
    }
}

public static class ServiceCollectionExcensions
{
    public static IServiceCollection AddCqs(this IServiceCollection services)
    {
        services.AddSingleton<IMediator, Mediator>();

        return services;
    }

    public static IServiceCollection AddHandler<THandler, TRequest>(this IServiceCollection services)
        where THandler : class, IRequestHandler<TRequest>
        where TRequest : class, IRequest
    {
        services.AddScoped<IRequestHandler<TRequest>, THandler>();
        return services;
    }

    public static IServiceCollection AddHandler<THandler, TRequest, TResponse>(this IServiceCollection services)
        where THandler : class, IRequestHandler<TRequest, TResponse>
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse
    {
        services.AddScoped<IRequestHandler<TRequest, TResponse>, THandler>();
        return services;
    }

    public static IServiceCollection AddMiddleware<TMiddleware>(this IServiceCollection services)
        where TMiddleware : class, IMediatorMiddleware
    {
        services.AddSingleton<TMiddleware, TMiddleware>();
        return services;
    }

    public static IServiceCollection AddMiddleware(this IServiceCollection services, IMediatorMiddleware middleware)
    {
        services.AddSingleton(middleware);
        return services;
    }
}