using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Networks.Filters;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.Core.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Core.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.Core.Stub
{
    internal class NetworkPerspectiveCoreStub : INetworkPerspectiveCore
    {
        private static readonly Guid NetworkId = new Guid("bd1bc916-db78-4e1e-b93b-c6feb8cf729e");
        private static readonly Guid ConnectorId = new Guid("04C753D8-FF9A-479C-B857-5D28C1EAF6C1");

        private readonly FileDataWriter _fileWriter;
        private readonly NetworkPerspectiveCoreConfig _config;

        public NetworkPerspectiveCoreStub(FileDataWriter fileWriter, IOptions<NetworkPerspectiveCoreConfig> config)
        {
            _fileWriter = fileWriter;
            _config = config.Value;
        }

        public Task<NetworkConfig> GetNetworkConfigAsync(SecureString accessToken, CancellationToken stoppingToken = default)
        {
            var customAttributes = new CustomAttributesConfig(
                groupAttributes: new[] { "NP-Test.Role" },
                propAttributes: new[] { "NP-Test.Employment_Date" },
                relationships: new[] { new CustomAttributeRelationship("NP-Test.FormalSupervisor", "Boss") });

            return Task.FromResult(new NetworkConfig(EmployeeFilter.Empty, customAttributes));
        }

        public async Task PushEntitiesAsync(SecureString accessToken, EmployeeCollection employees, DateTime changeDate, CancellationToken stoppingToken = default)
        {
            try
            {
                var entities = new List<HashedEntity>();

                foreach (var employee in employees.GetAllInternal())
                {
                    var entity = EntitiesMapper.ToEntity(employee, employees, changeDate, _config.DataSourceIdName);
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

        public async Task PushUsersAsync(SecureString accessToken, EmployeeCollection employees, CancellationToken stoppingToken = default)
        {
            try
            {
                var employeesList = employees
                    .GetAllInternal()
                    .Select(x => UsersMapper.ToUser(x, "test"));

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


        public Task ReportSyncSuccessfulAsync(SecureString accessToken, TimeRange timeRange, CancellationToken stoppingToken = default)
            => Task.CompletedTask;

        public Task<TokenValidationResponse> ValidateTokenAsync(SecureString accessToken, CancellationToken stoppingToken = default)
        {
            return Task.FromResult(new TokenValidationResponse(NetworkId, ConnectorId));
        }

        public IInteractionsStream OpenInteractionsStream(SecureString accessToken, CancellationToken stoppingToken = default)
            => new InteractionStreamStub(accessToken, _fileWriter, _config);
    }
}