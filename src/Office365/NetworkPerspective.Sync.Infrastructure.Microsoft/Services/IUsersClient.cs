﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users;

using NetworkPerspective.Sync.Application.Domain.Networks.Filters;
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
                            nameof(User.EmployeeId),
                            nameof(User.DisplayName),
                            nameof(User.Department),
                            nameof(User.CreatedDateTime)
                        },
                        Filter = "userType eq 'Member'",
                        Top = MaxPageSize,
                        Expand = new[]
                        {
                            nameof(User.Manager)
                        }
                    };
                }, stoppingToken);

            var pageIterator = PageIterator<User, UserCollectionResponse>
                .CreatePageIterator(_graphClient, usersResponse,
                async user =>
                {
                    var groups = await _graphClient
                        .Users[user.Id]
                        .TransitiveMemberOf
                        .GraphGroup
                        .GetAsync(x =>
                        {
                            x.QueryParameters = new()
                            {
                                Select = new[]
                                {
                                    nameof(Group.DisplayName),
                                }
                            };
                        });

                    var mails = new[] { user.Mail }
                        .Union(user.OtherMails);
                    var groupsNames = groups.Value.Select(x => x.DisplayName);

                    if (context.NetworkConfig.EmailFilter.IsInternal(mails, groupsNames))
                        result.Add(user);

                    return true;
                },
                request =>
                {
                    return request;
                });

            await pageIterator.IterateAsync(stoppingToken);

            var filteredResult = FilterUsers(context.NetworkConfig.EmailFilter, result);

            if (!filteredResult.Any())
                _logger.LogWarning("No users found in network '{networkId}'", context.NetworkId);
            else
                _logger.LogDebug("Fetching employees for network '{networkId}' completed. '{count}' employees found", context.NetworkId, filteredResult.Count);

            return filteredResult;
        }

        private List<User> FilterUsers(EmployeeFilter filter, List<User> users)
        {
            _logger.LogTrace("Filtering users based on network configuration. Input contains {count} users", users.Count);
            var result = users
                .Where(x => filter.IsInternal(x.Mail) || x.OtherMails.Any(filter.IsInternal))
                .ToList();

            return result;
        }

    }
}