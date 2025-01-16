using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Google.Apis.Admin.Directory.directory_v1;
using Google.Apis.Admin.Directory.directory_v1.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;

internal interface IUsersClient
{
    Task<IEnumerable<User>> GetUsersAsync(ICredential credentials, CancellationToken stoppingToken = default);
}

internal class UsersClient(IScopedStatusCache statusCache, IOptions<GoogleConfig> config, IRetryPolicyProvider retryPolicyProvider, ILogger<UsersClient> logger) : IUsersClient
{
    private const string TaskCaption = "Synchronizing users";
    private const string TaskDescription = "Fetching users data from Google API";

    private readonly GoogleConfig _config = config.Value;

    public async Task<IEnumerable<User>> GetUsersAsync(ICredential credentials, CancellationToken stoppingToken = default)
    {
        logger.LogDebug("Fetching users...");

        await statusCache.SetStatusAsync(SingleTaskStatus.WithUnknownProgress(TaskCaption, TaskDescription), stoppingToken);

        var retryPolicy = retryPolicyProvider.GetSecretRotationRetryPolicy();
        var users = await retryPolicy.ExecuteAsync(() => GetAllGoogleUsers(credentials, stoppingToken));

        return users;
    }

    private async Task<IList<User>> GetAllGoogleUsers(ICredential credentials, CancellationToken stoppingToken)
    {
        const string currentAccountCustomer = "my_customer";

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
            request.Customer = currentAccountCustomer;
            request.OrderBy = UsersResource.ListRequest.OrderByEnum.Email;
            request.PageToken = nextPageToken;
            request.Projection = UsersResource.ListRequest.ProjectionEnum.Full; // we do NOT know upfront what kind of custom section is set, so we cannot use ProjectionEnum.Custom
            var response = await retryPolicyProvider
                .GetThrottlingRetryPolicy()
                .ExecuteAsync(request.ExecuteAsync, stoppingToken);

            if (response.UsersValue != null)
                result.AddRange(response.UsersValue);

            nextPageToken = response.NextPageToken;

        } while (!string.IsNullOrEmpty(nextPageToken) && !stoppingToken.IsCancellationRequested);

        return result;
    }
}