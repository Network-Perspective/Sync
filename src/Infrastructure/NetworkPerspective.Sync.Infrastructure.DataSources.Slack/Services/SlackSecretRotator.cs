using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain.SecretRotation;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence;
using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services
{
    internal class SlackSecretsRotator : ISecretRotator
    {
        private readonly ILogger<SlackSecretsRotator> _logger;
        private readonly ISecretRotationService _secretRotationService;
        private readonly IUnitOfWork _unitOfWork;

        public SlackSecretsRotator(ISecretRotationService secretRotationService, IUnitOfWork unitOfWork, ILogger<SlackSecretsRotator> logger)
        {
            _logger = logger;
            _secretRotationService = secretRotationService;
            _unitOfWork = unitOfWork;
        }

        public async Task RotateSecrets()
        {
            _logger.LogInformation("Rotating Slack secrets");
            try
            {
                var connectors = await _unitOfWork
                    .GetConnectorRepository<SlackConnectorProperties>()
                    .GetAllAsync();

                foreach (var connector in connectors)
                {
                    _logger.LogInformation("Rotating token for connector {connectorId}", connector.Id);

                    var properties = connector.Properties.GetAll().ToDictionary(x => x.Key, x => x.Value);
                    var context = new SecretRotationContext(connector.Id, properties);
                    try
                    {
                        await _secretRotationService.ExecuteAsync(context);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to rotate token for connector {connectorId}", connector.Id);
                    }
                }

                _logger.LogInformation("Finished rotating slack secrets");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rotate Slack secrets");
                throw;
            }
        }

    }
}