using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

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

public interface IMediator
{
    Task SendAsync<TRequest>(TRequest request, CancellationToken stoppingToken = default)
        where TRequest : class, IRequest;

    Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken stoppingToken = default)
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse;
}

internal class Mediator(IServiceProvider serviceProvider) : IMediator
{

    public async Task SendAsync<TRequest>(TRequest request, CancellationToken stoppingToken)
        where TRequest : class, IRequest
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        await handler.HandleAsync(request, stoppingToken);
    }

    public async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request, CancellationToken stoppingToken)
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var handler = scope.ServiceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        return await handler.HandleAsync(request, stoppingToken);
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
}