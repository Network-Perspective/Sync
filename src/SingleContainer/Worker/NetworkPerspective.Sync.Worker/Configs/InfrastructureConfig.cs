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
                .WithName($"{configPath}__{nameof(Orchestrator)}__{nameof(OrchestratorHubClientConfig.BaseUrl)}")
                .WithMessage("Please provide url to NetworkPerspectiveOrchestrator with environment variable '{PropertyName}'");

            RuleFor(x => x.Core.BaseUrl)
                .NotEmpty()
                .WithName($"{configPath}__{nameof(Core)}__{nameof(NetworkPerspectiveCoreConfig.BaseUrl)}")
                .WithMessage("Please provide url to NetworkPerspectiveCore with environment variable '{PropertyName}'");

            RuleFor(x => x.Vaults)
                .SetValidator(new VaultsConfig.Validator($"{configPath}__{nameof(Vaults)}"));
        }
    }
}