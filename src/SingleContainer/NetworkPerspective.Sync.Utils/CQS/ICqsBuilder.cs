using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Utils.CQS.Commands;
using NetworkPerspective.Sync.Utils.CQS.Middlewares;
using NetworkPerspective.Sync.Utils.CQS.PreProcessors;
using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.CQS;

public interface ICqsBuilder
{
    ICqsBuilder AddHandler<THandler, TRequest, TResponse>()
        where THandler : class, IQueryHandler<TRequest, TResponse>
        where TRequest : class, IQuery<TResponse>
        where TResponse : class, IResponse;

    ICqsBuilder AddHandler<THandler, TRequest>()
        where THandler : class, ICommandHandler<TRequest>
        where TRequest : class, ICommand;

    ICqsBuilder AddMiddleware<TMiddleware>() where TMiddleware : class, IMediatorMiddleware;

    ICqsBuilder AddPreProcessor<TPreProcessor>() where TPreProcessor : class, IPreProcessor;
}

internal class CqsBuilder(IServiceCollection services) : ICqsBuilder
{
    ICqsBuilder ICqsBuilder.AddHandler<THandler, TRequest>()
    {
        services.AddScoped<ICommandHandler<TRequest>, THandler>();
        return this;
    }

    ICqsBuilder ICqsBuilder.AddHandler<THandler, TRequest, TResponse>()

    {
        services.AddScoped<IQueryHandler<TRequest, TResponse>, THandler>();
        return this;
    }

    ICqsBuilder ICqsBuilder.AddMiddleware<TMiddleware>()
    {
        services.AddScoped<IMediatorMiddleware, TMiddleware>();
        return this;
    }

    ICqsBuilder ICqsBuilder.AddPreProcessor<TPreProcessor>()
    {
        services.AddScoped<IPreProcessor, TPreProcessor>();
        return this;
    }
}