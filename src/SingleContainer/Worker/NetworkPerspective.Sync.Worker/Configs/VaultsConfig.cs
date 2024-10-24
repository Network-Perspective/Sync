using FluentValidation;

using NetworkPerspective.Sync.Infrastructure.Vaults.AmazonSecretsManager;
using NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;
using NetworkPerspective.Sync.Infrastructure.Vaults.GoogleSecretManager;
using NetworkPerspective.Sync.Infrastructure.Vaults.HashiCorpVault;

namespace NetworkPerspective.Sync.Worker.Configs;

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