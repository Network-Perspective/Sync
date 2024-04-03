using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Messages;

public record AddNetwork(Guid NetworkId) : IMessage;

public record NetworkAdded(Guid NetworkId) : IMessage;

public record Ping(string Message) : IMessage;

public record IsAuthenticated(string Message) : IRpcArgs;
public record IsAuthenticatedResult(string Message) : IRpcResult;

public record FindNetwork(Guid NetworkId) : IRpcArgs;
public record FindNetworkResult(Dictionary<string, string> Props) : IRpcResult;