using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.SingleContainer.Messages.Services;

public interface IRpcDispatcher
{
    Task<IRpcResult> CallRpc(string name, string payload, string returnType);
}

public class RpcDispatcher(IMessageSerializer messageSerializer, IServiceProvider serviceProvider,
    ILogger<RpcDispatcher> logger)
    : IRpcDispatcher
{
    public async Task<IRpcResult> CallRpc(string name, string payload, string returnType)
    {
        logger.LogDebug("Calling " + name + " with " + payload + " and " + returnType);
        try
        {
            var message = messageSerializer.Deserialize(name, payload);
            var resultClass = Type.GetType($"NetworkPerspective.Sync.SingleContainer.Messages.{returnType}");
            var handlerType = typeof(IRpcHandler<,>).MakeGenericType(message.GetType(), resultClass);
            var handler = serviceProvider.GetService(handlerType);
            if (handler == null)
            {
                logger.LogError("No handler found for {messageType}", name);
                throw new InvalidOperationException("No handler found for " + name);
            }
            var method = handler.GetType().GetMethod("HandleAsync", new[] { message.GetType() });
            if (method == null)
            {
                logger.LogError("No HandleAsync method found for {messageType}", name);
                throw new InvalidOperationException("No HandleAsync method found for " + name); 
            }
            var task = method.Invoke(handler, new object[] { message });
            
            return await (dynamic) task;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error dispatching message {messageType}", name);
            throw new InvalidOperationException("Error dispatching message " + name, e);
        }
    }

}