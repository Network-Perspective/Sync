using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class SetSecretsRequest : IRequest<AckDto>
{
    public string UserFriendlyName { get; set; } = "Set Secrets";
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public IDictionary<string, string> Secrets { get; set; }
}