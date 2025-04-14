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
                .WithName($"{configPath}__{nameof(SecretsPrefix)}")
                .WithMessage("Please provide secret prefix with environment variable '{PropertyName}'");

            RuleFor(x => x.Region)
                .NotEmpty()
                .WithName($"{configPath}__{nameof(Region)}")
                .WithMessage("Please provide region of AmazonSecretsManager with environment variable '{PropertyName}'");
        }
    }
}