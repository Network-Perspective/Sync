using System.Linq;

using FluentValidation;

using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Contract.V1.Impl;
using NetworkPerspective.Sync.Infrastructure.Core;
using NetworkPerspective.Sync.Infrastructure.Vaults.AmazonSecretsManager;
using NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;
using NetworkPerspective.Sync.Infrastructure.Vaults.GoogleSecretManager;
using NetworkPerspective.Sync.Infrastructure.Vaults.HashiCorpVault;

namespace NetworkPerspective.Sync.Worker;

public class WorkerConfiguration
{
    public InfrastructureConfig Infrastructure { get; set; }

    public class Validator : AbstractValidator<WorkerConfiguration>, IValidateOptions<WorkerConfiguration>
    {
        public ValidateOptionsResult Validate(string name, WorkerConfiguration options)
        {
            var validateResult = Validate(options);
            return validateResult.IsValid
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(validateResult.Errors.Select(x => x.ErrorMessage));
        }

        public Validator()
        {
            RuleFor(x => x.Infrastructure)
                .SetValidator(x => new InfrastructureConfig.Validator($"{nameof(Infrastructure)}"));
        }
    }
}

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

public class VaultsConfig
{
    public AzureKeyVaultConfig AzureKeyVault { get; set; }
    public HcpVaultConfig HcpVault { get; set; }
    public AmazonSecretsManagerConfig AmazonSecretsManager { get; set; }
    public GoogleSecretManagerConfig GoogleSecretManager { get; set; }

    public class Validator : AbstractValidator<VaultsConfig>
    {
        public Validator(string configPath)
        {
            RuleFor(x => x.AzureKeyVault)
                .SetValidator(new AzureKeyVaultConfig.Validator($"{configPath}:{nameof(AzureKeyVault)}"))
                .Unless(x => x.HcpVault is not null)
                .Unless(x => x.AmazonSecretsManager is not null)
                .Unless(x => x.GoogleSecretManager is not null);

            RuleFor(x => x.HcpVault)
                .SetValidator(new HcpVaultConfig.Validator($"{configPath}:{nameof(HcpVault)}"))
                .Unless(x => x.AzureKeyVault is not null)
                .Unless(x => x.AmazonSecretsManager is not null)
                .Unless(x => x.GoogleSecretManager is not null);

            RuleFor(x => x.AmazonSecretsManager)
                .SetValidator(new AmazonSecretsManagerConfig.Validator($"{configPath}:{nameof(AmazonSecretsManager)}"))
                .Unless(x => x.AzureKeyVault is not null)
                .Unless(x => x.HcpVault is not null)
                .Unless(x => x.GoogleSecretManager is not null);

            RuleFor(x => x.GoogleSecretManager)
                .SetValidator(new GoogleSecretManagerConfig.Validator($"{configPath}:{nameof(GoogleSecretManager)}"))
                .Unless(x => x.AzureKeyVault is not null)
                .Unless(x => x.HcpVault is not null)
                .Unless(x => x.AmazonSecretsManager is not null);
        }
    }
}