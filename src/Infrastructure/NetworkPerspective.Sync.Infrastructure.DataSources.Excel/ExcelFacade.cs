using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Mappers;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Excel;

public class ExcelFacade(IHashingService hashingService, IOptions<ExcelSyncConstraints> syncConstraints) : IDataSource
{
    private readonly ExcelSyncConstraints _syncConstraints = syncConstraints.Value;

    public Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
    {
        return Task.FromResult(SyncResult.Empty);
    }

    public Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        var incoming = context.Get<IEnumerable<EmployeeDto>>();
        var emailFilter = context.NetworkConfig.EmailFilter;

        var employees = incoming.ToList().ToDomainEmployees(emailFilter, hashingService.Hash);

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

        return Task.FromResult(new EmployeeCollection(employees, hashingService.Hash));
    }
}