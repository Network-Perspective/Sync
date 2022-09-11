using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Sync;

namespace NetworkPerspective.Sync.Application.Infrastructure.DataSources
{
    public interface IDataSource
    {
        Task<ISet<Interaction>> GetInteractions(SyncContext context, CancellationToken stoppingToken = default);
        Task<EmployeeCollection> GetEmployees(SyncContext context, CancellationToken stoppingToken = default);
        Task<EmployeeCollection> GetHashedEmployees(SyncContext context, CancellationToken stoppingToken = default);
        Task<bool> IsAuthorized(Guid networkId, CancellationToken stoppingToken = default);
    }
}