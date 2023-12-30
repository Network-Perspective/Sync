using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Infrastructure.Excel.Dtos;

namespace NetworkPerspective.Sync.Infrastructure.Excel.Services;

public class ExcelDataSourceFactory : IDataSourceFactory
{
    public Task<IDataSource> CreateAsync(Guid networkId, CancellationToken stoppingToken = default)
    {
        return Task.FromResult<IDataSource>(new ExcelDataSource());
    }
}

public class ExcelDataSource : IDataSource
{
    public Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
    {
        return Task.FromResult(new SyncResult(0, 0, new List<Exception>()));
    }

    public Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        var incoming = context.Get<List<EmployeeDto>>();
        var emailFilter = context.NetworkConfig.EmailFilter;

        var employees = incoming.ToDomainEmployees(emailFilter);
        employees.ForEach(e => e.EvaluateGroupAccess(context.HashFunction));

        return Task.FromResult(new EmployeeCollection(employees, null));
    }

    public Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        var incoming = context.Get<List<EmployeeDto>>();
        var emailFilter = context.NetworkConfig.EmailFilter;

        var employees = incoming.ToDomainEmployeesHashed(emailFilter);
        return Task.FromResult(new EmployeeCollection(employees, context.HashFunction));
    }
}