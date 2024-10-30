using System;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class InitializeOAuthRequest : IRequest
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public ConnectorDto Connector { get; set; }
    public string CallbackUri { get; set; }
}