using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class EmployeeEqualityComparer : IEqualityComparer<Employee>
    {
        private static readonly IEqualityComparer<string> StringEqualityComparer = StringComparer.InvariantCultureIgnoreCase;

        public bool Equals(Employee x, Employee y)
            => StringEqualityComparer.Equals(x.Id.PrimaryId, y.Id.PrimaryId) && StringEqualityComparer.Equals(x.Id.DataSourceId, y.Id.DataSourceId);

        public int GetHashCode(Employee obj)
            => obj.Id.PrimaryId.GetHashCode();
    }
}