using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Infrastructure.Core.HttpClients;
using NetworkPerspective.Sync.Infrastructure.Core.Mappers;
using NetworkPerspective.Sync.Infrastructure.Core.Services;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.Core.Exceptions;

namespace NetworkPerspective.Sync.Infrastructure.Core
{
    internal sealed class NetworkPerspectiveCoreFacade : INetworkPerspectiveCore
    {
        private readonly NetworkPerspectiveCoreConfig _npCoreConfig;
        private readonly ISyncHashedClient _client;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<NetworkPerspectiveCoreFacade> _logger;

        public NetworkPerspectiveCoreFacade(ISyncHashedClient client, IOptions<NetworkPerspectiveCoreConfig> npCoreConfig, ILoggerFactory loggerFactory)
        {
            _npCoreConfig = npCoreConfig.Value;
            _client = client;
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<NetworkPerspectiveCoreFacade>();
        }

        public IInteractionsStream OpenInteractionsStream(SecureString accessToken, string dataSourceIdName, CancellationToken stoppingToken = default)
            => new InteractionsStream(accessToken.Copy(), _client, _npCoreConfig, dataSourceIdName, _loggerFactory.CreateLogger<InteractionsStream>(), stoppingToken);

        public async Task PushUsersAsync(SecureString accessToken, EmployeeCollection employees, string dataSourceIdName, CancellationToken stoppingToken = default)
        {
            try
            {
                var employeesList = employees
                    .GetAllInternal()
                    .Select(x => UsersMapper.ToUser(x, dataSourceIdName));

                _logger.LogInformation("Pushing {count} users {url}", employeesList.Count(), _npCoreConfig.BaseUrl);

                var command = new SyncUsersCommand
                {
                    ServiceToken = accessToken.ToSystemString(),
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

        public async Task PushEntitiesAsync(SecureString accessToken, EmployeeCollection employees, DateTime changeDate, string dataSourceIdName, CancellationToken stoppingToken = default)
        {
            try
            {
                var employeesList = employees.GetAllInternal();

                _logger.LogInformation("Pushing {count} hashed entities to {url}", employeesList.Count(), _npCoreConfig.BaseUrl);

                var entities = new List<HashedEntity>();

                foreach (var employee in employeesList)
                {
                    var entity = EntitiesMapper.ToEntity(employee, employees, changeDate, dataSourceIdName);
                    entities.Add(entity);
                }

                var command = new SyncHashedEntitesCommand
                {
                    ServiceToken = accessToken.ToSystemString(),
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
                    ServiceToken = accessToken.ToSystemString(),
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

        public async Task<ConnectorConfig> GetNetworkConfigAsync(SecureString accessToken, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogDebug("Getting network settings from Core app...");
                var response = await _client.SettingsAsync(accessToken.ToSystemString(), stoppingToken);

                var emailFilter = new EmployeeFilter(response.Whitelist, response.Blacklist);
                _logger.LogDebug("Email filter: {emailFilter}", emailFilter.ToString());

                var customAttributeRelationships = response.CustomAttributes?.Relationship == null
                    ? Array.Empty<CustomAttributeRelationship>()
                    : response.CustomAttributes?.Relationship.Select(x => new CustomAttributeRelationship(x.PropName, x.RelationshipName));

                var customAttributes = new CustomAttributesConfig(
                    groupAttributes: response.CustomAttributes?.Group,
                    propAttributes: response.CustomAttributes?.Prop,
                    relationships: customAttributeRelationships);

                _logger.LogDebug("Custom attributes: {customAttributes}", customAttributes.ToString());


                return new ConnectorConfig(emailFilter, customAttributes);
            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException(_npCoreConfig.BaseUrl, ex);
            }
        }

        public async Task<ConnectorInfo> ValidateTokenAsync(SecureString accessToken, CancellationToken stoppingToken = default)
        {
            try
            {
                var result = await _client.QueryAsync(accessToken.ToSystemString(), stoppingToken);

                return new ConnectorInfo(result.ConnectorId.Value, result.NetworkId.Value);
            }
            catch (ApiException aex) when (aex.StatusCode == (int)HttpStatusCode.Forbidden)
            {
                throw new InvalidTokenException(_npCoreConfig.BaseUrl);
            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException(_npCoreConfig.BaseUrl, ex);
            }
        }

        public async Task ReportSyncStartAsync(SecureString accessToken, TimeRange timeRange, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogInformation("Notifying NetworkPerspective Core App sync started...");

                var command = new ReportSyncStartedCommand
                {
                    ServiceToken = accessToken.ToSystemString(),
                    SyncPeriodStart = timeRange.Start,
                    SyncPeriodEnd = timeRange.End,
                };

                await _client.ReportStartAsync(command, stoppingToken);

                _logger.LogInformation("NetworkPerspective Core App has been notified sync started");
            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException(_npCoreConfig.BaseUrl, ex);
            }
        }

        public async Task ReportSyncSuccessfulAsync(SecureString accessToken, TimeRange timeRange, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogInformation("Notifying NetworkPerspective Core App sync successed...");

                var command = new ReportSyncCompletedCommand
                {
                    ServiceToken = accessToken.ToSystemString(),
                    SyncPeriodStart = timeRange.Start,
                    SyncPeriodEnd = timeRange.End,
                    Success = true,
                    Message = string.Empty
                };

                await _client.ReportCompletedAsync(command, stoppingToken);

                _logger.LogInformation("NetworkPerspective Core App has been notified sync successed");
            }
            catch (Exception ex)
            {
                throw new NetworkPerspectiveCoreException(_npCoreConfig.BaseUrl, ex);
            }
        }

        public async Task TryReportSyncFailedAsync(SecureString accessToken, TimeRange timeRange, string message, CancellationToken stoppingToken = default)
        {
            try
            {
                _logger.LogInformation("Notifying NetworkPerspective Core App sync failed...");

                var command = new ReportSyncCompletedCommand
                {
                    ServiceToken = accessToken.ToSystemString(),
                    SyncPeriodStart = timeRange.Start,
                    SyncPeriodEnd = timeRange.End,
                    Success = false,
                    Message = message
                };

                await _client.ReportCompletedAsync(command, stoppingToken);

                _logger.LogInformation("NetworkPerspective Core App has been notified sync failed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "NetworkPerspective Core App has been NOT notified sync failed");
            }
        }
    }
}