using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using NetworkPerspective.Sync.Application.Domain.Connectors;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.HealthChecks
{
    public class PersistenceHealthCheck : IHealthCheck
    {
        private readonly IUnitOfWork _unitOfWork;

        public PersistenceHealthCheck(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await _unitOfWork
                    .GetConnectorRepository<ConnectorProperties>()
                    .GetAllAsync(cancellationToken);

                return HealthCheckResult.Healthy();
            }
            catch
            {
                return HealthCheckResult.Unhealthy();
            }
        }
    }
}