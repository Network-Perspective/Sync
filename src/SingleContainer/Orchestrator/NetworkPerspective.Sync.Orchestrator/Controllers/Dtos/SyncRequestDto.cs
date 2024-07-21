using System.Collections.Generic;

using NetworkPerspective.Sync.Orchestrator.Dtos;

namespace NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

public class SyncRequestDto
{
    public List<EmployeeDto> Employees { get; set; }
}