using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class EmployeeIdEqualityComparer : IEqualityComparer<EmployeeId>
    {
        private static readonly IEqualityComparer<string> StringEqualityComparer = StringComparer.InvariantCultureIgnoreCase;

        public bool Equals(EmployeeId x, EmployeeId y)
            => StringEqualityComparer.Equals(x.PrimaryId, y.PrimaryId) && StringEqualityComparer.Equals(x.DataSourceId, y.DataSourceId);

        public int GetHashCode(EmployeeId obj)
            => obj.PrimaryId.GetHashCode();
    }
}