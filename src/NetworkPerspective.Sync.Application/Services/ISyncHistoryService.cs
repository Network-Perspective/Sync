﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Sync;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface ISyncHistoryService
    {
        Task<DateTime> EvaluateSyncStartAsync(Guid networkId, CancellationToken stoppingToken = default);
        Task SaveLogAsync(SyncHistoryEntry syncHistoryEntry, CancellationToken stoppingToken = default);
    }

    internal class SyncHistoryService : ISyncHistoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClock _clock;
        private readonly SyncConfig _config;
        private readonly ILogger<SyncHistoryService> _logger;

        public SyncHistoryService(IUnitOfWork unitOfWork, IClock clock, IOptions<SyncConfig> config, ILogger<SyncHistoryService> logger)
        {
            _unitOfWork = unitOfWork;
            _clock = clock;
            _config = config.Value;
            _logger = logger;
        }

        public async Task<DateTime> EvaluateSyncStartAsync(Guid networkId, CancellationToken stoppingToken = default)
        {
            var lastSyncHistoryEntry = await _unitOfWork
                .GetSyncHistoryRepository()
                .FindLastLogAsync(networkId, stoppingToken);
            var lastSyncPeriodEnd = lastSyncHistoryEntry?.SyncPeriod.End;

            _logger.LogDebug("Last syncronization of network '{networkId}' {lastSync}", networkId, lastSyncPeriodEnd?.ToString() ?? "not found");

            return lastSyncPeriodEnd ?? _clock.UtcNow().AddDays(-_config.DefaultSyncLookbackInDays);
        }

        public async Task SaveLogAsync(SyncHistoryEntry syncHistoryEntry, CancellationToken stoppingToken = default)
        {
            _logger.LogDebug("Adding new {type} to persistence", typeof(SyncHistoryEntry));

            await _unitOfWork
                .GetSyncHistoryRepository()
                .AddAsync(syncHistoryEntry, stoppingToken);

            await _unitOfWork.CommitAsync(stoppingToken);

            _logger.LogDebug("Added {type} to persistence", typeof(SyncHistoryEntry));
        }
    }
}