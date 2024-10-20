using FluentValidation;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.AmazonSecretsManager;

public class AmazonSecretsManagerConfig
{
    public string SecretsPrefix { get; set; }
    public string Region { get; set; }

    public class Validator : AbstractValidator<AmazonSecretsManagerConfig>
    {
        public Validator(string configPath)
        {
            RuleFor(x => x.SecretsPrefix)
                .NotEmpty()
                .WithName($"{configPath}:{nameof(SecretsPrefix)}");

            RuleFor(x => x.Region)
                .NotEmpty()
                .WithName($"{configPath}:{nameof(Region)}");
        }
    }
}