﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Core.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Core.Mappers;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Extensions;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.Core.Exceptions;

namespace NetworkPerspective.Sync.Infrastructure.Core.Stub
{
    internal class NetworkPerspectiveCoreStub : INetworkPerspectiveCore
    {
        private static readonly Guid NetworkId = new("bd1bc916-db78-4e1e-b93b-c6feb8cf729e");
        private static readonly Guid ConnectorId = new("04C753D8-FF9A-479C-B857-5D28C1EAF6C1");

        private readonly FileDataWriter _fileWriter;
        private readonly NetworkPerspectiveCoreConfig _config;

        public NetworkPerspectiveCoreStub(FileDataWriter fileWriter, IOptions<NetworkPerspectiveCoreConfig> config)
        {
            _fileWriter = fileWriter;
            _config = config.Value;
        }

        public Task<ConnectorConfig> GetConnectorConfigAsync(SecureString accessToken, CancellationToken stoppingToken = default)
        {
            var customAttributes = new CustomAttributesConfig(
                groupAttributes: ["NP-Test.Role"],
                propAttributes: ["NP-Test.Employment_Date"],
                relationships: [new CustomAttributeRelationship("NP-Test.FormalSupervisor", "Boss")]);

            return Task.FromResult(new ConnectorConfig(EmployeeFilter.Empty, customAttributes));
        }

        public async Task PushEntitiesAsync(SecureString accessToken, EmployeeCollection employees, DateTime changeDate, string dataSourceIdName, CancellationToken stoppingToken = default)
        {
            try
            {
                var entities = new List<HashedEntity>();

                foreach (var employee in employees.GetAllInternal())
                {
                    var entity = EntitiesMapper.ToEntity(employee, employees, changeDate, dataSourceIdName);
                    entities.Add(entity);
                }

                var command = new SyncHashedEntitesCommand
                {
                    ServiceToken = accessToken.ToSystemString(),
                    Entites = entities
                };

                await _fileWriter.WriteAsync(command.Entites, $"{accessToken.ToSystemString().GetStableHashCode()}_entities.json", stoppingToken);
            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException("test", ex);
            }
        }

        public async Task PushGroupsAsync(SecureString accessToken, IEnumerable<Group> groups, CancellationToken stoppingToken = default)
        {
            try
            {
                var hashedGroups = groups
                    .Select(GroupsMapper.ToGroup)
                    .ToList();

                var command = new SyncHashedGroupStructureCommand
                {
                    ServiceToken = accessToken.ToSystemString(),
                    Groups = hashedGroups
                };

                await _fileWriter.WriteAsync(command.Groups, $"{accessToken.ToSystemString().GetStableHashCode()}_groups.json", stoppingToken);
            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException("test", ex);
            }
        }

        public async Task PushUsersAsync(SecureString accessToken, EmployeeCollection employees, string dataSourceIdName, CancellationToken stoppingToken = default)
        {
            try
            {
                var employeesList = employees
                    .GetAllInternal()
                    .Select(x => UsersMapper.ToUser(x, dataSourceIdName));

                var command = new SyncUsersCommand
                {
                    ServiceToken = accessToken.ToSystemString(),
                    Users = employeesList.ToList()
                };

                await _fileWriter.WriteAsync(command.Users, $"{accessToken.ToSystemString().GetStableHashCode()}_users.json", stoppingToken);
            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException("test", ex);
            }
        }

        public Task TryReportSyncFailedAsync(SecureString accessToken, TimeRange timeRange, string message, CancellationToken stoppingToken = default)
            => Task.CompletedTask;

        public Task ReportSyncStartAsync(SecureString accessToken, TimeRange timeRange, CancellationToken stoppingToken = default)
            => Task.CompletedTask;


        public Task ReportSyncSuccessfulAsync(SecureString accessToken, TimeRange timeRange, string message = null, CancellationToken stoppingToken = default)
            => Task.CompletedTask;

        public Task<CoreTokenValidationResult> ValidateTokenAsync(SecureString accessToken, CancellationToken stoppingToken = default)
        {
            return Task.FromResult(new CoreTokenValidationResult(ConnectorId, NetworkId));
        }

        public IInteractionsStream OpenInteractionsStream(SecureString accessToken, string dataSourceIdName, CancellationToken stoppingToken = default)
            => new InteractionStreamStub(accessToken, _fileWriter, _config, dataSourceIdName);
    }
}