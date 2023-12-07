using System.Collections.Generic;

using NetworkPerspective.Sync.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Infrastructure.Core.Mappers
{
    internal static class IdsMapper
    {
        public static IDictionary<string, string> ToIds(Employee employee, string dataSourceIdName)
        {
            var result = new Dictionary<string, string>
            {
                { "Email", employee.IsExternal ? "external" : employee.Id.PrimaryId},
                { dataSourceIdName, employee.IsExternal ? "external" : employee.Id.DataSourceId},
            };
            if (employee.Id.Username is not null && !employee.IsExternal)
                result.Add("Username", employee.Id.Username);
            return result;
        }
    }
}