using System.Collections.Generic;

using NetworkPerspective.Sync.Infrastructure.Excel.Dtos;

namespace NetworkPerspective.Sync.Excel.Controllers;

public class SyncRequestDto
{
    public List<EmployeeDto> Employees { get; set; } 
    public SyncMetadataDto Metadata { get; set; }
}