using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class InitializeOAuthRequest : IRequest
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Guid ConnectorId{ get; set; }
    public string ConnectorType { get; set; }
    public string CallbackUri { get; set; }
    public IDictionary<string, string> ConnectorProperties { get; set; }
}
