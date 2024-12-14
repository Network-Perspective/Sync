using System;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class InitializeOAuthRequest : IQuery<InitializeOAuthResponse>
{
    public string UserFriendlyName { get; set; } = "Initialize OAuth Flow";
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public ConnectorDto Connector { get; set; }
    public string CallbackUri { get; set; }
}