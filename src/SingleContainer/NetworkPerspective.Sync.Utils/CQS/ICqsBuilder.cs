using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Utils.CQS.Middlewares;
using NetworkPerspective.Sync.Utils.CQS.PreProcessors;
using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.CQS;

public interface ICqsBuilder
{
    ICqsBuilder AddHandler<THandler, TRequest, TResponse>()
        where THandler : class, IRequestHandler<TRequest, TResponse>
        where TRequest : class, IRequest<TResponse>
        where TResponse : class, IResponse;

    ICqsBuilder AddMiddleware<TMiddleware>() where TMiddleware : class, IMediatorMiddleware;
    ICqsBuilder AddPreProcessor<TPreProcessor>() where TPreProcessor : class, IPreProcessor;
}

internal class CqsBuilder(IServiceCollection services) : ICqsBuilder
{
    ICqsBuilder ICqsBuilder.AddHandler<THandler, TRequest, TResponse>()
    {
        services.AddScoped<IRequestHandler<TRequest, TResponse>, THandler>();
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