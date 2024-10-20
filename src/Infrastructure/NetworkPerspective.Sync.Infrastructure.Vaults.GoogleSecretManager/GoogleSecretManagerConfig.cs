using FluentValidation;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.GoogleSecretManager;

public class GoogleSecretManagerConfig
{
    public string ProjectId { get; set; }

    public class Validator : AbstractValidator<GoogleSecretManagerConfig>
    {
        public Validator(string configPath)
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty()
                .WithName($"{configPath}:{nameof(ProjectId)}");
        }
    }
}