using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using NetworkPerspective.Sync.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Application.Domain.Interactions
{
    public class InteractionVertexEqualityComparer : IEqualityComparer<InteractionVertex>
    {
        private readonly IEqualityComparer<EmployeeId> _employeeIdEqualityComparer;

        public InteractionVertexEqualityComparer(IEqualityComparer<EmployeeId> employeeIdEqualityComparer)
        {
            _employeeIdEqualityComparer = employeeIdEqualityComparer;
        }

        public bool Equals(InteractionVertex x, InteractionVertex y)
        {
            if (x == null || y == null)
                return false;

            return _employeeIdEqualityComparer.Equals(x.Id, y.Id);
        }

        public int GetHashCode([DisallowNull] InteractionVertex obj)
            => _employeeIdEqualityComparer.GetHashCode(obj.Id);
    }
}
