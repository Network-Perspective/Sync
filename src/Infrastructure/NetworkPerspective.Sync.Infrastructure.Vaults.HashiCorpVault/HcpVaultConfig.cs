using FluentValidation;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.HashiCorpVault;

public class HcpVaultConfig
{
    public string BaseUrl { get; set; }
    public string TestSecretName { get; set; }
    public string Token { get; set; }
    public string VaultRole { get; set; }
    public string MountPoint { get; set; }

    public class Validator : AbstractValidator<HcpVaultConfig>
    {
        public Validator(string configPath)
        {
            RuleFor(x => x.BaseUrl)
                .NotEmpty()
                .WithName($"{configPath}__{nameof(BaseUrl)}")
                .WithMessage("Please provide url to HashiCorpVault with environment variable '{PropertyName}'");

            RuleFor(x => x.TestSecretName)
                .NotEmpty()
                .WithName($"{configPath}__{nameof(TestSecretName)}")
                .WithMessage("Please provide test secret name with environment variable '{PropertyName}'");

            RuleFor(x => x.Token)
                .NotEmpty()
                .WithName($"{configPath}__{nameof(Token)}")
                .WithMessage("Please provide token to HashiCorpVault with environment variable '{PropertyName}'");

            RuleFor(x => x.VaultRole)
                .NotEmpty()
                .WithName($"{configPath}__{nameof(VaultRole)}")
                .WithMessage("Please provide vault role with environment variable '{PropertyName}'");

            RuleFor(x => x.MountPoint)
                .NotEmpty()
                .WithName($"{configPath}__{nameof(MountPoint)}")
                .WithMessage("Please provide secret engine mounting point with environment variable '{PropertyName}'");
        }
    }
}