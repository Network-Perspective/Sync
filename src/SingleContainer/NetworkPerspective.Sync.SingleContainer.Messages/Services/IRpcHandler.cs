namespace NetworkPerspective.Sync.SingleContainer.Messages.Services;

public interface IRpcHandler<in TArgs, TResult> where TArgs : IMessage where TResult : IMessage
{
    Task<TResult> HandleAsync(TArgs args);
}

public interface IRpcArgs : IMessage{ }
public interface IRpcResult : IMessage { }
