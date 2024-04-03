using System.Text.Json;

namespace NetworkPerspective.Sync.SingleContainer.Messages.Services;


public interface IMessageSerializer
{
    (string Name, string? Payload) Serialize(IMessage message);
    IMessage Deserialize(string name, string payload);
}

public class MessageSerializer : IMessageSerializer
{
    public (string Name, string? Payload) Serialize(IMessage message)
    {
        var json = JsonSerializer.Serialize((dynamic)message);
        return (message.GetType().Name, json);
    }

    public IMessage Deserialize(string name, string payload)
    {
        var type = Type.GetType($"NetworkPerspective.Sync.SingleContainer.Messages.{name}");
        // check if type implements IMessage
        if (type?.IsAssignableTo(typeof(IMessage)) != true)
        {
            throw new InvalidOperationException("Type does not implement IMessage");
        } 
        
        return (IMessage) JsonSerializer.Deserialize(payload, type)!;
    }
}