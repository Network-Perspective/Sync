using System;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class ConnectorStatusRequest : IRequest<ConnectorStatusResponse>, IConnectorScoped
{
    public string UserFriendlyName { get; set; } = "Get Status";
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public ConnectorDto Connector { get; set; }
}