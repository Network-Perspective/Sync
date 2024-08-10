using System.Collections.Generic;

using Google.Apis.Admin.Directory.directory_v1.Data;

using NetworkPerspective.Sync.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Mappers
{
    internal interface IEmployeesMapper
    {
        EmployeeCollection ToEmployees(IEnumerable<User> users);
    }
}