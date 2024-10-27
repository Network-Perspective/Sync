using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Worker.Application.Infrastructure.Core;

public interface INetworkPerspectiveCore
{
    IInteractionsStream OpenInteractionsStream(SecureString accessToken, string dataSourceIdName, CancellationToken stoppingToken = default);
    Task PushUsersAsync(SecureString accessToken, EmployeeCollection employees, string dataSourceIdName, CancellationToken stoppingToken = default);
    Task PushEntitiesAsync(SecureString accessToken, EmployeeCollection employees, DateTime changeDate, string dataSourceIdName, CancellationToken stoppingToken = default);
    Task PushGroupsAsync(SecureString accessToken, IEnumerable<Group> groups, CancellationToken stoppingToken = default);
    Task<ConnectorConfig> GetNetworkConfigAsync(SecureString accessToken, CancellationToken stoppingToken = default);
    Task ReportSyncStartAsync(SecureString accessToken, TimeRange timeRange, CancellationToken stoppingToken = default);
    Task ReportSyncSuccessfulAsync(SecureString accessToken, TimeRange timeRange, CancellationToken stoppingToken = default);
    Task TryReportSyncFailedAsync(SecureString accessToken, TimeRange timeRange, string message, CancellationToken stoppingToken = default);
    Task<CoreTokenValidationResult> ValidateTokenAsync(SecureString accessToken, CancellationToken stoppingToken = default);
}