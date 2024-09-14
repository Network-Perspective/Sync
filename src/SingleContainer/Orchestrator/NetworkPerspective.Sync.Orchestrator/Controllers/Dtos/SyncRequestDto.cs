using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

public class SyncRequestDto
{
    public List<EmployeeDto> Employees { get; set; }
}