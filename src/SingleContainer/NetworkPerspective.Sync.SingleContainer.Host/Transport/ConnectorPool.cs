using System.Collections.Concurrent;

using NetworkPerspective.Sync.SingleContainer.Messages;

namespace NetworkPerspective.Sync.SingleContainer.Host.Transport;

public interface IConnectorPool
{
    RegisterConnector GetConnectorRegistration(string connectionId);
    void RegisterConnector(string connectionId, RegisterConnector registration);
    public string FindConnectionId(string connectorName);
}

public class ConnectorPool : IConnectorPool
{
    private readonly ConcurrentDictionary<string, RegisterConnector> _registrations = new();
    private readonly ConcurrentDictionary<string, string> _name2ConnectionId = new();

    public void RegisterConnector(string connectionId, RegisterConnector registration)
    {
        _registrations[connectionId] = registration;
        _name2ConnectionId[registration.Name] = connectionId;
    }

    public string FindConnectionId(string connectorName)
    {
        return _name2ConnectionId.GetValueOrDefault(connectorName) ?? throw new InvalidOperationException();
    }

    public RegisterConnector GetConnectorRegistration(string connectionId)
    {
        var result = _registrations.GetValueOrDefault(connectionId);
        if (result == null)
        {
            throw new InvalidOperationException("Connector offline");
        }
        return result;
    }
}