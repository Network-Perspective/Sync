using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;

namespace NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;

public interface IDataSource
{
    Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default);
    Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default);
    Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default);
}