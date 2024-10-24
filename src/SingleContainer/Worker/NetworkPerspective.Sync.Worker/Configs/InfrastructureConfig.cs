using FluentValidation;

using NetworkPerspective.Sync.Contract.V1.Impl;
using NetworkPerspective.Sync.Infrastructure.Core;

namespace NetworkPerspective.Sync.Worker.Configs;

public class InfrastructureConfig
{
    public OrchestratorHubClientConfig Orchestrator { get; set; }
    public NetworkPerspectiveCoreConfig Core { get; set; }
    public VaultsConfig Vaults { get; set; }

    public class Validator : AbstractValidator<InfrastructureConfig>
    {
        public Validator(string configPath)
        {
            RuleFor(x => x.Orchestrator.BaseUrl)
                .NotEmpty()
                .WithName($"{configPath}:{nameof(Orchestrator)}:{nameof(OrchestratorHubClientConfig.BaseUrl)}");

            RuleFor(x => x.Core.BaseUrl)
                .NotEmpty()
                .WithName($"{configPath}:{nameof(Core)}:{nameof(NetworkPerspectiveCoreConfig.BaseUrl)}");

            RuleFor(x => x.Vaults)
                .SetValidator(new VaultsConfig.Validator($"{configPath}:{nameof(Vaults)}"));
        }
    }
}