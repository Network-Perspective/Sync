using FluentValidation;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault;

public class AzureKeyVaultConfig
{
    public string BaseUrl { get; set; }
    public string TestSecretName { get; set; } = "hashing-key";

    public class Validator : AbstractValidator<AzureKeyVaultConfig>
    {
        public Validator(string configPath)
        {
            RuleFor(x => x.BaseUrl)
                .NotEmpty()
                .WithName($"{configPath}__{nameof(BaseUrl)}")
                .WithMessage("Please provide url to AzureKeyVault with environment variable '{PropertyName}'");
        }
    }
}