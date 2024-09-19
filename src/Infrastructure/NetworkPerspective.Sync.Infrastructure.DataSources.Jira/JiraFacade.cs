using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira;

internal class JiraFacade : IDataSource
{
    public Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
        => Task.FromResult(SyncResult.Empty);
}