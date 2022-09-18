using System.Collections.Generic;

using NetworkPerspective.Sync.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Infrastructure.Core.Mappers
{
    internal static class IdsMapper
    {
        public static IDictionary<string, string> ToIds(Employee employee, string dataSourceIdName)
        {
            return new Dictionary<string, string>
            {
                { "Email", employee.IsExternal ? "external" : employee.Email },
                { dataSourceIdName, employee.IsExternal ? "external" : employee.SourceInternalId },
            };
        }
    }
}