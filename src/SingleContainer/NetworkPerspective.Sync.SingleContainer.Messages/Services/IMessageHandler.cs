namespace NetworkPerspective.Sync.SingleContainer.Messages.Services;

public interface IMessageHandler<in T> where T : IMessage
{
    Task HandleAsync(T msg);
}