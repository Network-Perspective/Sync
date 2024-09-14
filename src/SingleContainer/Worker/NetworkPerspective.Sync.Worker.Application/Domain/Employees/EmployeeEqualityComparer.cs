using System.Collections.Generic;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Employees
{
    public class EmployeeEqualityComparer : IEqualityComparer<Employee>
    {
        public bool Equals(Employee x, Employee y)
        {
            if (x is null || y is null)
                return false;

            if (ReferenceEquals(x, y))
                return true;

            if (x.GetType() != y.GetType())
                return false;

            return EmployeeId.EqualityComparer.Equals(x.Id, y.Id);
        }

        public int GetHashCode(Employee obj)
            => obj.Id.GetHashCode();
    }
}