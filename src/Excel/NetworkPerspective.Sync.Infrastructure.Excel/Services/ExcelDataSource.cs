using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Infrastructure.Excel.Dtos;

namespace NetworkPerspective.Sync.Infrastructure.Excel.Services;

public class ExcelDataSource : IDataSource
{
    private readonly ExcelSyncConstraints _syncConstraints;

    public ExcelDataSource(IOptions<ExcelSyncConstraints> syncConstraints)
    {
        _syncConstraints = syncConstraints.Value;
    }
    
    public Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
    {
        return Task.FromResult(new SyncResult(0, 0, new List<Exception>()));
    }

    public Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        var incoming = context.Get<List<EmployeeDto>>();
        var emailFilter = context.NetworkConfig.EmailFilter;

        var employees = incoming.ToDomainEmployees(emailFilter, context.HashFunction);

        return Task.FromResult(new EmployeeCollection(employees, null));
    }

    public Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        var incoming = context.Get<List<EmployeeDto>>();
        var emailFilter = context.NetworkConfig.EmailFilter;

        var employees = incoming.ToDomainEmployeesHashed(emailFilter);

        // validate constraints
        if (employees.Count < _syncConstraints.MinRecordsAccepted)
            throw new ValidationException("Operation does not meet the minimum records constraint.");
        
        return Task.FromResult(new EmployeeCollection(employees, context.HashFunction));
    }
}