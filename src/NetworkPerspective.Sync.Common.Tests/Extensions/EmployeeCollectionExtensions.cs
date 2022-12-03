using System.Collections.Generic;
using System.Collections.Immutable;

using NetworkPerspective.Sync.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Common.Tests.Extensions
{
    public static class EmployeeCollectionExtensions
    {
        public static List<Employee> Add(this List<Employee> employees, string mail)
        {
            employees.Add(Employee.CreateInternal(EmployeeId.Create(mail, mail), ImmutableArray<Group>.Empty));
            return employees;
        }
    }
}