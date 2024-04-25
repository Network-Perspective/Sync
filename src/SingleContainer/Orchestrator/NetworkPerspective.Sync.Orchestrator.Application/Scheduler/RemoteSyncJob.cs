using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using Quartz;

namespace NetworkPerspective.Sync.Orchestrator.Application.Scheduler
{
    internal class RemoteSyncJob : IJob
    {
        private readonly ILogger<RemoteSyncJob> _logger;

        public RemoteSyncJob(ILogger<RemoteSyncJob> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            // networkid / connectorId / datasourceId
            var id = Guid.Parse(context.JobDetail.Key.Name);
            _logger.LogInformation("Triggered job to order sync... to... using...");

            return Task.CompletedTask;
        }
    }
}
