using System;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class InitializeOAuthResponse : IResponse
{
    public Guid CorrelationId { get; set; }
    public string AuthUri { get; set; }
    public string State { get; set; }
    public DateTime StateExpirationTimestamp { get; set; }
}