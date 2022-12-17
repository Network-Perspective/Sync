using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Criterias;

namespace NetworkPerspective.Sync.Infrastructure.Google.Services
{
    internal interface IUsersClient
    {
        Task<IEnumerable<User>> GetUsersAsync(NetworkConfig networkConfig, CancellationToken stoppingToken = default);
        Task<bool> CanGetUsersAsync(GoogleCredential credentials, CancellationToken stoppingToken = default);
    }

    internal sealed class UsersClient : IUsersClient
    {
        private const string TaskCaption = "Synchronizing users";
        private const string TaskDescription = "Fetching users data from Google API";

        private readonly Network<GoogleNetworkProperties> _network;
        private readonly GoogleConfig _config;
        private readonly GoogleCredential _googleCredential;
        private readonly ITasksStatusesCache _tasksStatusesCache;
        private readonly IEnumerable<ICriteria> _criterias;
        private readonly ILogger<UsersClient> _logger;
        private readonly IThrottlingRetryHandler _retryHandler = new ThrottlingRetryHandler();

        public UsersClient(Network<GoogleNetworkProperties> network, GoogleCredential googleCredential, ITasksStatusesCache tasksStatusesCache, IOptions<GoogleConfig> config, IEnumerable<ICriteria> criterias, ILogger<UsersClient> logger)
        {
            _config = config.Value;
            _network = network;
            _googleCredential = googleCredential;
            _tasksStatusesCache = tasksStatusesCache;
            _criterias = criterias;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetUsersAsync(NetworkConfig networkConfig, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Fetching users for network '{networkId}'...", _network.NetworkId);

            await _tasksStatusesCache.SetStatusAsync(_network.NetworkId, new SingleTaskStatus(TaskCaption, TaskDescription, null), stoppingToken);

            var users = await GetAllGoogleUsers(_network, stoppingToken);
            var filteredUsers = FilterUsers(networkConfig.EmailFilter, users);

            if (!filteredUsers.Any())
                _logger.LogWarning("No users found in network '{networkId}'", _network.NetworkId);
            else
                _logger.LogDebug("Fetching employees for network '{networkId}' completed. '{count}' employees found", _network.NetworkId, filteredUsers.Count());

            return filteredUsers;
        }

        public async Task<bool> CanGetUsersAsync(GoogleCredential credentials, CancellationToken stoppingToken = default)
        {
            var userCredentials = credentials
                .CreateWithUser(_network.Properties.AdminEmail)
                .UnderlyingCredential as ServiceAccountCredential;

            var service = new DirectoryService(new BaseClientService.Initializer
            {
                HttpClientInitializer = userCredentials,
                ApplicationName = _config.ApplicationName,
            });

            var request = service.Users.List();
            request.MaxResults = 10;
            request.Domain = _network.Properties.Domain;
            request.OrderBy = UsersResource.ListRequest.OrderByEnum.Email;
            var response = await _retryHandler.ExecuteAsync(request.ExecuteAsync, _logger, stoppingToken);
            return response.UsersValue != null;
        }

        private async Task<IList<User>> GetAllGoogleUsers(Network<GoogleNetworkProperties> network, CancellationToken stoppingToken)
        {
            var userCredentials = _googleCredential
                .CreateWithUser(network.Properties.AdminEmail)
                .UnderlyingCredential as ServiceAccountCredential;

            var service = new DirectoryService(new BaseClientService.Initializer
            {
                HttpClientInitializer = userCredentials,
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
                var response = await _retryHandler.ExecuteAsync(request.ExecuteAsync, _logger, stoppingToken);

                if (response.UsersValue != null)
                    result.AddRange(response.UsersValue);

                nextPageToken = response.NextPageToken;

            } while (!string.IsNullOrEmpty(nextPageToken) && !stoppingToken.IsCancellationRequested);

            return result;
        }

        private IEnumerable<User> FilterUsers(EmailFilter emailFilter, IList<User> employeesProfiles)
        {
            foreach (var criteria in _criterias)
                employeesProfiles = criteria.MeetCriteria(employeesProfiles);

            var filteredProfiles = employeesProfiles.Where(x => emailFilter.IsInternalUser(x.PrimaryEmail));

            return filteredProfiles;
        }
    }
}