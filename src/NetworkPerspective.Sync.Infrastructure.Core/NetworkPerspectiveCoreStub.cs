using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Core.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Core;
using NetworkPerspective.Sync.Infrastructure.Core.Mappers;

namespace NetworkPerspective.Sync.Application.Infrastructure.Core
{
    public class NetworkPerspectiveCoreStub : INetworkPerspectiveCore
    {
        private static readonly Guid NetworkId = new Guid("bd1bc916-db78-4e1e-b93b-c6feb8cf729e");
        private static readonly Guid ConnectorId = new Guid("04C753D8-FF9A-479C-B857-5D28C1EAF6C1");

        public Task<NetworkConfig> GetNetworkConfigAsync(SecureString accessToken, CancellationToken stoppingToken = default)
        {
            var customAttributes = new CustomAttributesConfig(
                groupAttributes: new[] { "NP-Test.Role" },
                propAttributes: new[] { "NP-Test.Employment_Date" },
                relationships: new[] { new CustomAttributeRelationship("NP-Test.FormalSupervisor", "Boss") });

            return Task.FromResult(new NetworkConfig(EmailFilter.Empty, customAttributes));
        }

        public async Task PushEntitiesAsync(SecureString accessToken, EmployeeCollection employees, DateTime changeDate, CancellationToken stoppingToken = default)
        {
            try
            {
                var entities = new List<HashedEntity>();

                foreach (var employee in employees.GetAllInternal())
                {
                    var entity = EntitiesMapper.ToEntity(employee, employees, changeDate, "test");
                    entities.Add(entity);
                }

                var command = new SyncHashedEntitesCommand
                {
                    ServiceToken = new NetworkCredential(string.Empty, accessToken).Password,
                    Entites = entities
                };

                await new FileDataWriter("Data").WriteAsync(command.Entites, $"{accessToken.ToSystemString().GetStableHashCode()}_entities.json", stoppingToken);
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
                    ServiceToken = new NetworkCredential(string.Empty, accessToken).Password,
                    Groups = hashedGroups
                };

                await new FileDataWriter("Data").WriteAsync(command.Groups, $"{accessToken.ToSystemString().GetStableHashCode()}_groups.json", stoppingToken);
            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException("test", ex);
            }
        }

        public async Task PushInteractionsAsync(SecureString accessToken, ISet<Interaction> interactions, CancellationToken stoppingToken = default)
        {
            try
            {
                var interactionsToPush = interactions
                        .Select(x => InteractionMapper.DomainIntractionToDto(x, "test"))
                        .ToList();

                var command = new SyncHashedInteractionsCommand
                {
                    ServiceToken = new NetworkCredential(string.Empty, accessToken).Password,
                    Interactions = interactionsToPush
                };

                if (interactions.Any())
                    await new FileDataWriter("Data").WriteAsync(command.Interactions, $"{accessToken.ToSystemString().GetStableHashCode()}_{interactions.First().Timestamp:yyyyMMdd_HHmmss}_interactions.json", stoppingToken);
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
                    ServiceToken = new NetworkCredential(string.Empty, accessToken).Password,
                    Users = employeesList.ToList()
                };

                await new FileDataWriter("Data").WriteAsync(command.Users, $"{accessToken.ToSystemString().GetStableHashCode()}_users.json", stoppingToken);
            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException("test", ex);
            }
        }

        public Task<TokenValidationResponse> ValidateTokenAsync(SecureString accessToken, CancellationToken stoppingToken = default)
        {
            return Task.FromResult(new TokenValidationResponse(NetworkId, ConnectorId));
        }
    }
}