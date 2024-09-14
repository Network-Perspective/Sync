using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Statuses;

public sealed class AuthTesterContext
{
    public Guid ConnectorId { get; }
    public string ConnectorType { get; }
    public IDictionary<string, string> ConnectorProperties { get; }

    public AuthTesterContext(Guid connectorId, string connectorType, IDictionary<string, string> connectorProperties)
    {
        ConnectorId = connectorId;
        ConnectorType = connectorType;
        ConnectorProperties = connectorProperties;
    }
}