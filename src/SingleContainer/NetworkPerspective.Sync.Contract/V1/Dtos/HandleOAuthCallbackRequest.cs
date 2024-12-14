using System;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class HandleOAuthCallbackRequest : IRequest<AckDto>
{
    public string UserFriendlyName { get; set; } = "Handle OAuth Callback";
    public Guid CorrelationId { get; set; }
    public string Code { get; set; }
    public string State { get; set; }
}