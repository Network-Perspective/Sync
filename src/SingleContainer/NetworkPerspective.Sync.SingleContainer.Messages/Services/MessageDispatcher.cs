using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.SingleContainer.Messages.Services;

public interface IMessageDispatcher
{
    Task DispatchMessage(string name, string payload);
}

public class MessageDispatcher(IMessageSerializer messageSerializer, IServiceProvider serviceProvider,
    ILogger<MessageDispatcher> logger)
    : IMessageDispatcher
{
    public async Task DispatchMessage(string name, string payload)
    {
        logger.LogDebug("Dispatching " + name + " with " + payload);
        try
        {
            var message = messageSerializer.Deserialize(name, payload);
            var handlerType = typeof(IMessageHandler<>).MakeGenericType(message.GetType());
            var handler = serviceProvider.GetService(handlerType);
            if (handler == null)
            {
                logger.LogError("No handler found for {messageType}", name);
                return;
            }
            var method = handler.GetType().GetMethod("HandleAsync", new[] { message.GetType() });
            if (method == null)
            {
                logger.LogError("No HandleAsync method found for {messageType}", name);
                return;
            }
            var task = method.Invoke(handler, new object[] { message });
            if (task is Task t)
            {
                await t;
            }
            else
            {
                logger.LogWarning("HandleAsync method for {messageType} did not return a Task", name);
            }
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error dispatching message {messageType}", name);
        }
    }



}