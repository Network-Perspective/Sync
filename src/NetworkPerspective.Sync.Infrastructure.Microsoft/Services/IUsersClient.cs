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
        private const int MaxPageSize = 999;
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

            var result = new List<User>();

            var usersResponse = await _graphClient
                .Users
                .GetAsync(x =>
                {
                    x.QueryParameters = new UsersRequestBuilder.UsersRequestBuilderGetQueryParameters
                    {
                        Select = new[]
                        {
                            nameof(User.Id),
                            nameof(User.Mail),
                            nameof(User.OtherMails),
                            nameof(User.DisplayName),
                            nameof(User.Department),
                            nameof(User.CreatedDateTime)
                        },
                        Top = MaxPageSize
                    };
                }, stoppingToken);

            var pageIterator = PageIterator<User, UserCollectionResponse>
                .CreatePageIterator(_graphClient, usersResponse,
                user =>
                {
                    result.Add(user);
                    return true;
                },
                request =>
                {
                    return request;
                });

            await pageIterator.IterateAsync(stoppingToken);

            if (!result.Any())
                _logger.LogWarning("No users found in network '{networkId}'", context.NetworkId);
            else
                _logger.LogDebug("Fetching employees for network '{networkId}' completed. '{count}' employees found", context.NetworkId, usersResponse.Value.Count);

            return result;
        }
    }
}