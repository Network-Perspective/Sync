using System;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public interface IResponse
{
    public Guid CorrelationId { get; set; }
}
