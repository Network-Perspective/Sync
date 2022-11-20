using System.Collections.Generic;

namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class EmployeeEqualityComparer : IEqualityComparer<Employee>
    {
        private readonly IEqualityComparer<EmployeeId> _employeeIdEqualityComparer;

        public EmployeeEqualityComparer(IEqualityComparer<EmployeeId> employeeIdEqualityComparer)
        {
            _employeeIdEqualityComparer = employeeIdEqualityComparer;
        }

        public bool Equals(Employee x, Employee y)
            => _employeeIdEqualityComparer.Equals(x.Id, y.Id);

        public int GetHashCode(Employee obj)
            => obj.Id.PrimaryId.GetHashCode();
    }
}