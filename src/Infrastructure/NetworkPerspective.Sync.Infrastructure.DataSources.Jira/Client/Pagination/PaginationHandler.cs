using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Pagination;

// https://developer.atlassian.com/cloud/jira/platform/rest/v3/intro/#pagination
internal class PaginationHandler(ILogger<PaginationHandler> logger)
{
    public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity, TResponse>(
        Func<int, CancellationToken, Task<TResponse>> callApi,
        CancellationToken stoppingToken = default)
        where TResponse : IPaginatedResponse<TEntity>
    {
        var entities = new List<TEntity>();
        bool isLast;

        do
        {
            logger.LogTrace("Calling api with parameter '{Parameter}':'{Value}'", nameof(IPaginatedResponse<TEntity>.StartAt), entities.Count);
            var response = await callApi(entities.Count, stoppingToken);
            logger.LogTrace("Got {Count} item/s from api", response.Values.Count);

            isLast = response.IsLast;
            entities.AddRange(response.Values);

        } while (!isLast && !stoppingToken.IsCancellationRequested);

        return entities;
    }
}