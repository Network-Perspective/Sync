using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Messages;

public record Ping(string Message) : IMessage;

public record IsAuthenticated(string Message) : IRpcArgs;
public record IsAuthenticatedResult(string Message) : IRpcResult;