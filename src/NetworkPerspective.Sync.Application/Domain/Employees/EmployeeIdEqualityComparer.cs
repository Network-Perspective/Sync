using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class EmployeeIdEqualityComparer : IEqualityComparer<EmployeeId>
    {
        private static readonly IEqualityComparer<string> StringEqualityComparer = StringComparer.InvariantCultureIgnoreCase;

        public bool Equals(EmployeeId x, EmployeeId y)
        {
            if (x is null || y is null)
                return false;

            if (ReferenceEquals(x, y))
                return true;

            if (x.GetType() != y.GetType())
                return false;

            return StringEqualityComparer.Equals(x.PrimaryId, y.PrimaryId);
        }

        public int GetHashCode([DisallowNull] EmployeeId obj)
            => obj.GetHashCode();
    }
}