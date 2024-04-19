using System.Text.Json;


namespace NetworkPerspective.Sync.SingleContainer.Messages.CQS;


public interface IMessageSerializer
{
    string Serialize<TMessage>(TMessage message)
        where TMessage : IMessage;
    TMessage Deserialize<TMessage>(string payload)
        where TMessage : IMessage;
}

public class MessageSerializer : IMessageSerializer
{
    public string Serialize<TMessage>(TMessage message)
        where TMessage : IMessage
    {
        var json = JsonSerializer.Serialize(message);
        return json;
    }

    public TMessage Deserialize<TMessage>(string payload)
        where TMessage : IMessage
    {
        return JsonSerializer.Deserialize<TMessage>(payload);
    }
}