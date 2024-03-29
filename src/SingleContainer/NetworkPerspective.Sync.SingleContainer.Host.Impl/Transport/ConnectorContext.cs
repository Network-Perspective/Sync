namespace NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;

public interface IConnectorContext
{
    public string Name { get; }
    public ConnectorFamily Family { get; }
}

public interface IConnectorContextProvider
{
    string ConnectionId { get; }
    public IConnectorContext Current { get; }
    public void Initialize(string connectionId);
}

public class ConnectorContext(string Name, ConnectorFamily Family) : IConnectorContext
{
    public string Name { get; private set; } = Name;
    public ConnectorFamily Family { get; private set; } = Family;
}

public class ConnectorContextProvider(IConnectorPool connectorPool) : IConnectorContextProvider
{
    public string ConnectionId { get; private set; } = null!;

    public IConnectorContext Current
    {
        get
        {
            if (ConnectionId == null)
            {
                throw new InvalidOperationException("Connector context not initialized");
            }
            var registration = connectorPool.GetConnectorRegistration(this.ConnectionId);
            return new ConnectorContext(registration.Name, registration.Family);
        }
    }

    public void Initialize(string connectionId)
    {
        ConnectionId = connectionId;
    }
}

// public interface IConnectorContextProvider
// {
// }
//
// public class ConnectorContextProvider : IConnectorContextProvider
// {
//     public IConnectorContext Create(string name, ConnectorFamily family)
//     {
//         return new ConnectorContext(name, family);
//     }
// }
//