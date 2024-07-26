using System.Collections.Generic;

using NetworkPerspective.Sync.Contract.V1.Dtos;

namespace NetworkPerspective.Sync.Excel.Controllers;

public class SyncRequestDto
{
    public List<EmployeeDto> Employees { get; set; }
}