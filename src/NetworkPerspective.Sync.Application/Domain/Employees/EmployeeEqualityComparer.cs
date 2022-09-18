using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class EmployeeEqualityComparer : IEqualityComparer<Employee>
    {
        private static readonly IEqualityComparer<string> StringEqualityComparer = StringComparer.InvariantCultureIgnoreCase;

        public bool Equals(Employee x, Employee y)
            => StringEqualityComparer.Equals(x.Email, y.Email) && StringEqualityComparer.Equals(x.SourceInternalId, y.SourceInternalId);

        public int GetHashCode(Employee obj)
            => obj.Email.GetHashCode();
    }
}