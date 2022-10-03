using System.Collections.Immutable;

using NetworkPerspective.Sync.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Common.Tests.Extensions
{
    public static class EmployeeCollectionExtensions
    {
        public static EmployeeCollection Add(this EmployeeCollection employees, string mail)
        {
            employees.Add(Employee.CreateInternal(EmployeeId.Create(mail, mail), ImmutableArray<Group>.Empty), ImmutableHashSet<string>.Empty);
            return employees;
        }
    }
}