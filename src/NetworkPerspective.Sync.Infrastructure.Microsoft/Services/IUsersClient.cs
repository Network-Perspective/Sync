using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users;

using NetworkPerspective.Sync.Application.Domain.Sync;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Services
{
    internal interface IUsersClient
    {
        Task<IEnumerable<User>> GetUsersAsync(SyncContext contex, CancellationToken stoppingToken = default);
    }

    internal class UsersClient : IUsersClient
    {
        private readonly GraphServiceClient _graphClient;
        private readonly ILogger<UsersClient> _logger;

        public UsersClient(GraphServiceClient graphClient, ILogger<UsersClient> logger)
        {
            _graphClient = graphClient;
            _logger = logger;
        }

        public async Task<IEnumerable<User>> GetUsersAsync(SyncContext context, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Fetching users for network '{networkId}'...", context.NetworkId);

            var usersResponse = await _graphClient
                .Users
                .GetAsync(x =>
                {
                    x.QueryParameters = new UsersRequestBuilder.UsersRequestBuilderGetQueryParameters
                    {
                        Select = new[] { "Id", "Mail", "OtherMails" }
                    };
                }, stoppingToken);

            if (!usersResponse.Value.Any())
                _logger.LogWarning("No users found in network '{networkId}'", context.NetworkId);
            else
                _logger.LogDebug("Fetching employees for network '{networkId}' completed. '{count}' employees found", context.NetworkId, usersResponse.Value.Count);

            return usersResponse.Value;
        }
    }
}