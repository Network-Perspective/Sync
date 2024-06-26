﻿using System;
using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Utils.Models;

namespace NetworkPerspective.Sync.Application.Infrastructure.Core
{
    public interface INetworkPerspectiveCore
    {
        IInteractionsStream OpenInteractionsStream(SecureString accessToken, CancellationToken stoppingToken = default);
        Task PushUsersAsync(SecureString accessToken, EmployeeCollection employees, CancellationToken stoppingToken = default);
        Task PushEntitiesAsync(SecureString accessToken, EmployeeCollection employees, DateTime changeDate, CancellationToken stoppingToken = default);
        Task PushGroupsAsync(SecureString accessToken, IEnumerable<Group> groups, CancellationToken stoppingToken = default);
        Task<NetworkConfig> GetNetworkConfigAsync(SecureString accessToken, CancellationToken stoppingToken = default);
        Task ReportSyncStartAsync(SecureString accessToken, TimeRange timeRange, CancellationToken stoppingToken = default);
        Task ReportSyncSuccessfulAsync(SecureString accessToken, TimeRange timeRange, CancellationToken stoppingToken = default);
        Task TryReportSyncFailedAsync(SecureString accessToken, TimeRange timeRange, string message, CancellationToken stoppingToken = default);
        Task<TokenValidationResponse> ValidateTokenAsync(SecureString accessToken, CancellationToken stoppingToken = default);
    }
}