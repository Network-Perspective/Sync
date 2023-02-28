using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    internal class MicrosoftFacade : IDataSource
    {
        public Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            return Task.FromResult(new EmployeeCollection(Array.Empty<Employee>(), x => x));
        }

        public Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            return Task.FromResult(new EmployeeCollection(Array.Empty<Employee>(), x => x));
        }

        public Task<bool> IsAuthorizedAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            return Task.FromResult(true);
        }

        public Task SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
        {
            return Task.CompletedTask;
        }
    }
}