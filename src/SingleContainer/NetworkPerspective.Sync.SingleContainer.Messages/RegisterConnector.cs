using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Messages;

[Flags]
public enum ConnectorFamily
{
    GSuite, Slack, Office365, Excel
}

public record RegisterConnector(string Name, ConnectorFamily Family) : IMessage;