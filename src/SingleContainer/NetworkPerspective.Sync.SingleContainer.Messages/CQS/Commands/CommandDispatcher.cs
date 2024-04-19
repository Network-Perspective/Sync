using System;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.SingleContainer.Messages.CQS.Commands;

public interface ICommandDispatcher
{
    Task Dispatch<TCommand>(TCommand command) where TCommand : ICommand;
}

internal class CommandDispatcher(IServiceProvider serviceProvider, ILogger<CommandDispatcher> logger)
    : ICommandDispatcher
{
    public async Task Dispatch<TCommand>(TCommand command)
        where TCommand : ICommand
    {
        logger.LogDebug("Dispatching command '{Type}'", typeof(TCommand));
        try
        {
            var handler = serviceProvider.GetService<ICommandHandler<TCommand>>();
            if (handler == null)
            {
                logger.LogError("No handler found for '{CommandType}' make sure handler '{HandlerType}'is registered in DI container", typeof(TCommand), typeof(ICommandHandler<TCommand>));
                return;
            }

            await handler.HandleAsync(command);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error dispatching command '{CommandType}'", typeof(TCommand));
        }
    }
}