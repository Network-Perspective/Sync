using System;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class RotateSecretsRequest : IRequest<AckDto>, IConnectorScoped
{
    public string UserFriendlyName { get; set; } = "Rotate Secrets";
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public ConnectorDto Connector { get; set; }
}