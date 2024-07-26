using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Worker.Application.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.Excel;

public class ExcelFacade : IDataSource
{
    private readonly ExcelSyncConstraints _syncConstraints;

    public ExcelFacade(IOptions<ExcelSyncConstraints> syncConstraints)
    {
        _syncConstraints = syncConstraints.Value;
    }

    public Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
    {
        return Task.FromResult(SyncResult.Empty);
    }

    public Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        var incoming = context.Get<IEnumerable<EmployeeDto>>();
        var emailFilter = context.NetworkConfig.EmailFilter;

        var employees = incoming.ToList().ToDomainEmployees(emailFilter, context.HashFunction);

        return Task.FromResult(new EmployeeCollection(employees, null));
    }

    public Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        var incoming = context.Get<IEnumerable<EmployeeDto>>();
        var emailFilter = context.NetworkConfig.EmailFilter;

        var employees = incoming.ToList().ToDomainEmployeesHashed(emailFilter);

        // validate constraints
        if (employees.Count() < _syncConstraints.MinRecordsAccepted)
            throw new ValidationException("Operation does not meet the minimum records constraint.");

        return Task.FromResult(new EmployeeCollection(employees, context.HashFunction));
    }
}