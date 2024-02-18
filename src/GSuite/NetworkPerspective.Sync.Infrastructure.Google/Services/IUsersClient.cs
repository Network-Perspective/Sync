using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Networks.Filters;
using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Criterias;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal interface IUsersClient
    {
        Task<IEnumerable<User>> GetUsersAsync(Network<GoogleNetworkProperties> network, NetworkConfig networkConfig, CancellationToken stoppingToken = default);
    }

    internal sealed class UsersClient : IUsersClient
    {
        private const string TaskCaption = "Synchronizing users";
        private const string TaskDescription = "Fetching users data from Google API";

        private readonly GoogleConfig _config;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly IEnumerable<ICriteria> _criterias;
        private readonly IRetryPolicyProvider _retryPolicyProvider;
        private readonly ICredentialsProvider _credentialsProvider;
        private readonly ILogger<UsersClient> _logger;

        public UsersClient(ITasksStatusesCache tasksStatusesCache, IOptions<GoogleConfig> config, IEnumerable<ICriteria> criterias, IRetryPolicyProvider retryPolicyProvider, ICredentialsProvider credentialsProvider, ILogger<UsersClient> logger)
        {
            _config = config.Value;
            _tasksStatusesCache = tasksStatusesCache;
            _criterias = criterias;
            _retryPolicyProvider = retryPolicyProvider;
            _credentialsProvider = credentialsProvider;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetUsersAsync(Network<GoogleNetworkProperties> network, NetworkConfig networkConfig, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Fetching users for network '{networkId}'...", network.NetworkId);

            await _tasksStatusesCache.SetStatusAsync(network.NetworkId, new SingleTaskStatus(TaskCaption, TaskDescription, null), stoppingToken);

            var retryPolicy = _retryPolicyProvider.GetSecretRotationRetryPolicy();
            var users = await retryPolicy.ExecuteAsync(() => GetAllGoogleUsers(network, stoppingToken));

            var filteredUsers = FilterUsers(networkConfig.EmailFilter, users);

            if (!filteredUsers.Any())
                _logger.LogWarning("No users found in network '{networkId}'", network.NetworkId);
            else
                _logger.LogDebug("Fetching employees for network '{networkId}' completed. '{count}' employees found", network.NetworkId, filteredUsers.Count());

            return filteredUsers;
        }

        private async Task<IList<User>> GetAllGoogleUsers(Network<GoogleNetworkProperties> network, CancellationToken stoppingToken)
        {
            var credentials = await _credentialsProvider.GetForUserAsync(network.Properties.AdminEmail, stoppingToken);

            var service = new DirectoryService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credentials,
                ApplicationName = _config.ApplicationName,
            });

            var result = new List<User>();
            var nextPageToken = string.Empty;
            do
            {
                var request = service.Users.List();
                request.MaxResults = 500;
                request.Domain = network.Properties.Domain;
                request.OrderBy = UsersResource.ListRequest.OrderByEnum.Email;
                request.PageToken = nextPageToken;
                request.Projection = UsersResource.ListRequest.ProjectionEnum.Full; // we do NOT know upfront what kind of custom section is set, so we cannot use ProjectionEnum.Custom
                var response = await _retryPolicyProvider
                    .GetThrottlingRetryPolicy()
                    .ExecuteAsync(request.ExecuteAsync, stoppingToken);

                if (response.UsersValue != null)
                    result.AddRange(response.UsersValue);

                nextPageToken = response.NextPageToken;

            } while (!string.IsNullOrEmpty(nextPageToken) && !stoppingToken.IsCancellationRequested);

            return result;
        }

        private IEnumerable<User> FilterUsers(EmployeeFilter emailFilter, IList<User> employeesProfiles)
        {
            foreach (var criteria in _criterias)
                employeesProfiles = criteria.MeetCriteria(employeesProfiles);

            var filteredProfiles = employeesProfiles
                .Where(x => emailFilter.IsInternal(x.PrimaryEmail)
                    || x.Aliases != null && x.Aliases.Any(emailFilter.IsInternal));

            return filteredProfiles;
        }
    }
}