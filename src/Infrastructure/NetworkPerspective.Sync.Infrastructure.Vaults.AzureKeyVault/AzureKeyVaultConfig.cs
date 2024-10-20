using FluentValidation;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;

public class AzureKeyVaultConfig
{
    public string BaseUrl { get; set; }
    public string TestSecretName { get; set; }

    public class Validator : AbstractValidator<AzureKeyVaultConfig>
    {
        public Validator(string configPath)
        {
            RuleFor(x => x.BaseUrl)
                .NotEmpty()
                .WithName($"{configPath}:{nameof(BaseUrl)}");

            RuleFor(x => x.TestSecretName)
                .NotEmpty()
                .WithName($"{configPath}:{nameof(TestSecretName)}");
        }
    }
}