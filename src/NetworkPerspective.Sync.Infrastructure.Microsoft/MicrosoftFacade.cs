﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Mappers;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    internal sealed class MicrosoftFacade : IDataSource
    {
        private readonly IUsersClient _usersClient;
        private readonly ILogger<MicrosoftFacade> _logger;

        public MicrosoftFacade(IUsersClient usersClient, ILogger<MicrosoftFacade> logger)
        {
            _usersClient = usersClient;
            _logger = logger;
        }

        public async Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Getting employees for network '{networkId}'", context.NetworkId);

            var users = await _usersClient.GetUsersAsync(context, stoppingToken);

            return EmployeesMapper.ToEmployees(users);
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