using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Infrastructure.Excel.Dtos;

namespace NetworkPerspective.Sync.Infrastructure.Excel.Services;

public static class EmployeeDtoMapper
{
    public static List<Employee> ToDomainEmployees(
        this List<EmployeeDto> dtos,
        SyncMetadataIncludesDto metadata,
        EmailFilter emailFilter
    )
    {
        var employeeIdEmailDict = new Dictionary<string, string>();
        foreach (EmployeeDto dto in dtos)
        {
            if (employeeIdEmailDict.ContainsKey(dto.EmployeeId)) continue;
            employeeIdEmailDict.Add(dto.EmployeeId, dto.Email);
        }

        var result = new List<Employee>();
        foreach (EmployeeDto dto in dtos)
        {
            var isInternal = emailFilter.IsInternalUser(dto.Email);

            if (!isInternal)
            {
                result.Add(Employee.CreateExternal(dto.Email));
                continue;
            }

            // construct props
            var props = new Dictionary<string, object>();
            if (dto.Props != null)
            {
                foreach (var prop in dto.Props)
                {
                    if (!metadata.Props.Contains(prop.Name)) continue;
                    if (props.ContainsKey(prop.Name)) continue;
                    props.Add(prop.Name, prop.Value);
                }
            }
            if (dto.EmploymentDate != default)
            {
                props.Add("EmploymentDate", dto.EmploymentDate);
            }

            // construct groups
            var groups = new List<Group>();
            if (dto.Groups != null)
            {
                foreach (var g in dto.Groups)
                {
                    if (!metadata.Groups.Contains(g.Category)) continue;
                    groups.Add(Group.CreateWithParentId(
                        id: g.Id,
                        name: g.Name,
                        category: g.Category,
                        parentId: g.ParentId
                    ));
                }
            }

            // construct relations
            var relations = new List<Relation>();
            if (dto.Relationships != null)
            {
                foreach (EmployeeRelationshipDto rel in dto.Relationships)
                {
                    if (!metadata.Relationships.Contains(rel.RelationshipName)) continue;

                    var email = rel.Email;
                    if (string.IsNullOrWhiteSpace(email))
                    {
                        if (string.IsNullOrWhiteSpace(rel.EmployeeId)) continue;
                        if (!employeeIdEmailDict.ContainsKey(rel.EmployeeId)) continue;
                        email = employeeIdEmailDict[rel.EmployeeId];
                    }
                    relations.Add(Relation.Create(
                        name: rel.RelationshipName ?? Relation.SupervisorRelationName,
                        targetEmployeeEmail: email
                    ));
                }
            }

            var employee = Employee.CreateInternal(
                id: EmployeeId.Create(dto.Email, dto.EmployeeId),
                groups: groups,
                props: props,
                relations: new RelationsCollection(relations)
            );
            result.Add(employee);
        }

        return result;
    }
}