using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Infrastructure.Core.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Core.Mappers;

namespace NetworkPerspective.Sync.Infrastructure.Core
{
    internal sealed class NetworkPerspectiveCoreFacade : INetworkPerspectiveCore
    {
        private readonly NetworkPerspectiveCoreConfig _npCoreConfig;
        private readonly ISyncHashedClient _client;
        private readonly ILogger<NetworkPerspectiveCoreFacade> _logger;

        public NetworkPerspectiveCoreFacade(ISyncHashedClient client, IOptions<NetworkPerspectiveCoreConfig> npCoreConfig, ILogger<NetworkPerspectiveCoreFacade> logger)
        {
            _npCoreConfig = npCoreConfig.Value;
            _client = client;
            _logger = logger;
        }

        public async Task PushInteractionsAsync(SecureString accessToken, ISet<Interaction> interactions, CancellationToken stoppingToken = default)
        {
            try
            {
                if (!interactions.Any())
                {
                    _logger.LogInformation("Skipping pushing interaction because there is nothing to push...");
                    return;
                }

                _logger.LogInformation("Pushing {count} hashed interactions to {url}", interactions.Count, _npCoreConfig.BaseUrl);

                var dataPartitionsCount = Math.Ceiling(interactions.Count / (double)_npCoreConfig.MaxInteractionsPerRequestCount);

                for (int i = 0; i < dataPartitionsCount; i++)
                {
                    var interactionsToPush = interactions
                            .Skip(_npCoreConfig.MaxInteractionsPerRequestCount * i)
                            .Take(_npCoreConfig.MaxInteractionsPerRequestCount)
                            .Select(x => InteractionMapper.DomainIntractionToDto(x, _npCoreConfig.DataSourceIdName))
                            .ToList();

                    var command = new SyncHashedInteractionsCommand
                    {
                        ServiceToken = new NetworkCredential(string.Empty, accessToken).Password,
                        Interactions = interactionsToPush
                    };

                    var response = await _client.SyncInteractionsAsync(command, stoppingToken);

                    _logger.LogInformation("Uploaded interactions batch {x} of {y} with count {count} hashed interactions to {url} successfully, correlationId: '{correlationId}'",
                        (i + 1), dataPartitionsCount, interactionsToPush.Count, _npCoreConfig.BaseUrl, response);
                }
            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException(_npCoreConfig.BaseUrl, ex);
            }
        }

        public async Task PushUsersAsync(SecureString accessToken, EmployeeCollection employees, CancellationToken stoppingToken = default)
        {
            try
            {
                var employeesList = employees
                    .GetAllInternal()
                    .Select(x => UsersMapper.ToUser(x, _npCoreConfig.DataSourceIdName));

                _logger.LogInformation("Pushing {count} users {url}", employeesList.Count(), _npCoreConfig.BaseUrl);

                var command = new SyncUsersCommand
                {
                    ServiceToken = new NetworkCredential(string.Empty, accessToken).Password,
                    Users = employeesList.ToList()
                };

                var response = await _client.SyncUsersAsync(command, stoppingToken);

                _logger.LogInformation("Uploaded {count} users {url} successfully, correlationId: '{correlationId}'", employeesList.Count(), _npCoreConfig.BaseUrl, response);

            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException(_npCoreConfig.BaseUrl, ex);
            }
        }

        public async Task PushEntitiesAsync(SecureString accessToken, EmployeeCollection employees, CancellationToken stoppingToken = default)
        {
            try
            {
                var employeesList = employees.GetAllInternal();

                _logger.LogInformation("Pushing {count} hashed entities to {url}", employeesList.Count(), _npCoreConfig.BaseUrl);

                var entities = new List<HashedEntity>();

                foreach (var employee in employeesList)
                {
                    var manager = employees.Find(employee.ManagerEmail);
                    entities.Add(EntitiesMapper.ToEntity(employee, manager, _npCoreConfig.DataSourceIdName));
                }

                var command = new SyncHashedEntitesCommand
                {
                    ServiceToken = new NetworkCredential(string.Empty, accessToken).Password,
                    Entites = entities
                };

                var response = await _client.SyncEntitiesAsync(command, stoppingToken);

                _logger.LogInformation("Uploaded {count} hashed entities to {url} successfully, correlationId: '{correlationId}'", employeesList.Count(), _npCoreConfig.BaseUrl, response);
            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException(_npCoreConfig.BaseUrl, ex);
            }
        }

        public async Task PushGroupsAsync(SecureString accessToken, IEnumerable<Group> groups, CancellationToken stoppingToken = default)
        {
            try
            {
                var hashedGroups = groups
                    .Where(x => !string.IsNullOrEmpty(x.Name))
                    .Select(GroupsMapper.ToGroup)
                    .ToList();

                _logger.LogInformation("Pushing {count} hashed groups to {url}", hashedGroups.Count, _npCoreConfig.BaseUrl);

                var command = new SyncHashedGroupStructureCommand
                {
                    ServiceToken = new NetworkCredential(string.Empty, accessToken).Password,
                    Groups = hashedGroups
                };

                var response = await _client.SyncGroupsAsync(command, stoppingToken);

                _logger.LogInformation("Uploaded {count} hashed groups to {url} successfully, correlationId: '{correlationId}'", hashedGroups.Count, _npCoreConfig.BaseUrl, response);
            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException(_npCoreConfig.BaseUrl, ex);
            }
        }

        public async Task<NetworkConfig> GetNetworkConfigAsync(SecureString accessToken, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogDebug("Getting network settings from Core app...");
                var response = await _client.SettingsAsync(new NetworkCredential(string.Empty, accessToken).Password, stoppingToken);
                _logger.LogDebug("Whitelist users: {whitelist}", string.Join(", ", response.Whitelist));
                _logger.LogDebug("Blacklist users: {blacklist}", string.Join(", ", response.Blacklist));
                var emailFilter = new EmailFilter(response.Whitelist, response.Blacklist);
                // some logging what came from Core App
                var customAttributes = new CustomAttributesConfig(
                    groupAttributes: new[] { "Role" },
                    propAttributes: new[] { "Employment_Date" },
                    pathAttributes: new[] { "Formal_Group" }
                    );

                return new NetworkConfig(emailFilter, customAttributes);
            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException(_npCoreConfig.BaseUrl, ex);
            }
        }

        public async Task<TokenValidationResponse> ValidateTokenAsync(SecureString accessToken, CancellationToken stoppingToken = default)
        {
            try
            {
                var result = await _client.QueryAsync(new NetworkCredential(string.Empty, accessToken).Password, stoppingToken);

                return new TokenValidationResponse(result.NetworkId.Value, result.ConnectorId.Value);
            }
            catch (ApiException aex) when (aex.StatusCode == StatusCodes.Status403Forbidden)
            {
                throw new InvalidTokenException(_npCoreConfig.BaseUrl);
            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException(_npCoreConfig.BaseUrl, ex);
            }
        }
    }
}