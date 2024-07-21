using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class Employee
{
    public string Email { get; set; }
    public string EmployeeId { get; set; }
    public string Name { get; set; }
    public DateTime EmploymentDate { get; set; }
    public List<EmployeeProp> Props { get; set; }
    public List<EmployeeGroup> Groups { get; set; }
    public List<EmployeeGroup> Permissions { get; set; }
    public List<EmployeeRelationship> Relationships { get; set; }
}

public class EmployeeRelationship
{
    public string Email { get; set; }
    public string EmployeeId { get; set; }
    public string RelationshipName { get; set; }
}

public class EmployeeGroup
{
    public string Category { get; set; }
    public string Name { get; set; }
    public string Id { get; set; }
    public string ParentId { get; set; }
}

public class EmployeeProp
{
    public string Name { get; set; }
    public string Value { get; set; }
}