﻿using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Init
{
    internal class DbInitializerHostedService : IHostedService
    {
        private readonly IDbInitializer _dbInitializer;
        private readonly ILogger<DbInitializerHostedService> _logger;

        public DbInitializerHostedService(IDbInitializer dbInitializer, ILogger<DbInitializerHostedService> logger)
        {
            _dbInitializer = dbInitializer;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting Db initialization...");
            await _dbInitializer.InitializeAsync();
            _logger.LogInformation("Db initialized!");
        }

        public Task StopAsync(CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}