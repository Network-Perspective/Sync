using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    /// <see href="https://api.slack.com/docs/pagination"/>
    internal class CursorPaginationHandler
    {
        private readonly ILogger<CursorPaginationHandler> _logger;

        public CursorPaginationHandler(ILogger<CursorPaginationHandler> logger)
        {
            _logger = logger;
        }

        public async Task<IEnumerable<TEntity>> GetAllAsync<TEntity, TResponse>(
            Func<string, CancellationToken, Task<TResponse>> callApi,
            Func<TResponse, IEnumerable<TEntity>> getEnties,
            CancellationToken stoppingToken)
            where TResponse : ICursorPagination
        {
            var entities = new List<TEntity>();
            var nextCursor = string.Empty;
            do
            {
                _logger.LogTrace($"Calling api with parameter {nameof(nextCursor)}: '{nextCursor}'");
                var response = await callApi(nextCursor, stoppingToken);
                var responseEntities = getEnties(response);

                _logger.LogTrace($"Got {responseEntities.Count()} from api");
                entities.AddRange(responseEntities);
                nextCursor = response.Metadata?.NextCursor ?? string.Empty;

                _logger.LogTrace($"Next page cursor is '{nextCursor}'");
            } while (nextCursor != string.Empty && !stoppingToken.IsCancellationRequested);

            return entities;
        }
    }
}