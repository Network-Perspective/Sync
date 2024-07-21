using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Dtos;

public class EmployeeDto
{
    public string Email { get; set; }
    public string EmployeeId { get; set; }
    public string Name { get; set; }
    public DateTime EmploymentDate { get; set; }
    public List<EmployeePropDto> Props { get; set; }
    public List<EmployeeGroupDto> Groups { get; set; }
    public List<EmployeeGroupDto> Permissions { get; set; }
    public List<EmployeeRelationshipDto> Relationships { get; set; }
}

public class EmployeeRelationshipDto
{
    public string Email { get; set; }
    public string EmployeeId { get; set; }
    public string RelationshipName { get; set; }
}

public class EmployeeGroupDto
{
    public string Category { get; set; }
    public string Name { get; set; }
    public string Id { get; set; }
    public string ParentId { get; set; }
}

public class EmployeePropDto
{
    public string Name { get; set; }
    public string Value { get; set; }
}