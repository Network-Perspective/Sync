using System;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class HandleOAuthCallbackRequest : IRequest
{
    public Guid CorrelationId { get; set; }
    public string Code { get; set; }
    public string State { get; set; }
}
