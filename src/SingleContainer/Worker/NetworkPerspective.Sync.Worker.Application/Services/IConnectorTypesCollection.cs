using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Exceptions;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IConnectorTypesCollection
{
    ConnectorType this[string name] { get; }

    IEnumerable<string> GetTypesNames();
}

internal class ConnectorTypesCollection(IEnumerable<ConnectorType> dataSources) : IConnectorTypesCollection
{
    public ConnectorType this[string name]
    {
        get
        {
            var connectorType = dataSources.FirstOrDefault(x => x.Name == name);

            return connectorType is not null
                ? connectorType
                : throw new InvalidConnectorTypeException(name);
        }
    }

    public IEnumerable<string> GetTypesNames()
    {
        return dataSources.Select(x => x.Name);
    }
}