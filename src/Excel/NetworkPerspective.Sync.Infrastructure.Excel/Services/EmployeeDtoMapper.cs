using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Networks.Filters;
using NetworkPerspective.Sync.Infrastructure.Excel.Dtos;

namespace NetworkPerspective.Sync.Infrastructure.Excel.Services;

public static class EmployeeDtoMapper
{
    public static List<Employee> ToDomainEmployees(
        this List<EmployeeDto> dtos,
        EmployeeFilter emailFilter,
        HashFunction.Delegate hash // Required to provide already hashed groupAccess
    )
    {
        var result = new List<Employee>();
        foreach (EmployeeDto dto in dtos)
        {
            var isInternal = emailFilter.IsInternal(dto.Email);

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
                    props.TryAdd(prop.Name, prop.Value);
                }
            }
            props.TryAdd(Employee.PropKeyName, dto.Name);

            // construct groupsAccess from permissions
            var groupAccess = dto.Permissions == null
                ? new List<string>()
                : dto.Permissions.Select(x => hash(x.Id)).ToList();

            // construct relations
            var relations = GetRelations(dto, dtos);

            var employee = Employee.CreateInternal(
                id: EmployeeId.Create(dto.Email, dto.EmployeeId),
                groups: Enumerable.Empty<Group>(),
                props: props,
                relations: relations,
                groupAccess: groupAccess
            );
            result.Add(employee);
        }

        return result;
    }

    public static List<Employee> ToDomainEmployeesHashed(
        this List<EmployeeDto> dtos,
        EmployeeFilter emailFilter
    )
    {
        var result = new List<Employee>();
        foreach (EmployeeDto dto in dtos)
        {
            var isInternal = emailFilter.IsInternal(dto.Email);

            if (!isInternal)
            {
                result.Add(Employee.CreateExternal(dto.Email));
                continue;
            }

            // construct props
            var props = new Dictionary<string, object>();
            if (dto.EmploymentDate != default)
            {
                props.TryAdd(Employee.PropKeyEmploymentDate, dto.EmploymentDate);
            }

            // construct groups
            var groups = new List<Group>();
            if (dto.Groups != null)
            {
                foreach (var g in dto.Groups)
                {
                    groups.Add(Group.CreateWithParentId(
                        id: g.Id,
                        name: g.Name,
                        category: g.Category,
                        parentId: g.ParentId
                    ));
                }
            }

            // construct relations
            var relations = GetRelations(dto, dtos);

            var employee = Employee.CreateInternal(
                id: EmployeeId.Create(dto.Email, dto.EmployeeId),
                groups: groups,
                props: props,
                relations: relations
            );
            result.Add(employee);
        }

        return result;
    }

    private static RelationsCollection GetRelations(EmployeeDto dto, List<EmployeeDto> all)
    {
        var employeeIdEmailDict = new Dictionary<string, string>();
        foreach (EmployeeDto e in all)
        {
            if (e.EmployeeId == null || employeeIdEmailDict.ContainsKey(e.EmployeeId)) continue;
            employeeIdEmailDict.Add(e.EmployeeId, e.Email);
        }

        var relations = new List<Relation>();
        if (dto.Relationships != null)
        {
            foreach (EmployeeRelationshipDto rel in dto.Relationships)
            {
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

        return new RelationsCollection(relations);
    }
}