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
                .WithName($"{configPath}:{nameof(BaseUrl)}");

            RuleFor(x => x.TestSecretName)
                .NotEmpty()
                .WithName($"{configPath}:{nameof(TestSecretName)}");

            RuleFor(x => x.Token)
                .NotEmpty()
                .WithName($"{configPath}:{nameof(Token)}");

            RuleFor(x => x.VaultRole)
                .NotEmpty()
                .WithName($"{configPath}:{nameof(VaultRole)}");

            RuleFor(x => x.MountPoint)
                .NotEmpty()
                .WithName($"{configPath}:{nameof(MountPoint)}");
        }
    }
}