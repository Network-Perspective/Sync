using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class SyncRequestDto : IRequest
{
    public Guid CorrelationId { get; set; }
    public Guid ConnectorId { get; set; }
    public List<EmployeeDto> Employees { get; set; }
}