using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.SingleContainer.Messages.CQS.Queries;

public interface IQueryDispatcher
{
    Task<TQueryResult> Dispatch<TQueryArgs, TQueryResult>(TQueryArgs args)
        where TQueryArgs : IQueryArgs
        where TQueryResult : IQueryResult;
}

internal class QueryDispatcher(IServiceProvider serviceProvider, ILogger<QueryDispatcher> logger)
    : IQueryDispatcher
{
    public async Task<TQueryResult> Dispatch<TQueryArgs, TQueryResult>(TQueryArgs args)
        where TQueryArgs : IQueryArgs
        where TQueryResult : IQueryResult
    {
        logger.LogDebug("Dispatching query '{Type}'...", typeof(TQueryArgs));
        try
        {
            var handler = serviceProvider.GetService<IQueryHandler<TQueryArgs, TQueryResult>>();
            if (handler == null)
            {
                logger.LogError("No handler found for '{QueryType}'", typeof(IQueryHandler<TQueryArgs, TQueryResult>));
                throw new InvalidOperationException("No handler found for " + typeof(IQueryHandler<TQueryArgs, TQueryResult>));
            }


            return await handler.HandleAsync(args);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error dispatching query '{QueryType}'", typeof(TQueryArgs));
            throw new InvalidOperationException("Error dispatching query " + typeof(TQueryArgs), ex);
        }
    }

}