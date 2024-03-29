using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Messages;

public record AddNetwork(Guid NetworkId) : IMessage;

public record NetworkAdded(Guid NetworkId) : IMessage;