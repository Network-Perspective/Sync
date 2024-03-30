using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Messages;


public record FindNetwork(Guid NetworkId) : IRpcArgs;
public record FindNetworkResult(Dictionary<string, string> Props) : IRpcResult;