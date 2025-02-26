using FluentValidation;

using Microsoft.ApplicationInsights.WorkerService;

namespace NetworkPerspective.Sync.Worker.ApplicationInsights;

public class ApplicationInsightConfig : ApplicationInsightsServiceOptions
{
    public string RoleName { get; set; }
    public string RoleInstance { get; set; }

    public class Validator : AbstractValidator<ApplicationInsightConfig>
    {
        public Validator(string configPath)
        {
            RuleFor(x => x.ConnectionString)
                .NotEmpty()
                .WithName($"{configPath}__{nameof(ApplicationInsightConfig)}__{nameof(ConnectionString)}")
                .WithMessage("Please provide connection string to ApplicationInsights with environment variable '{PropertyName}'")
                .WithSeverity(Severity.Warning);

            RuleFor(x => x.RoleName)
                .NotEmpty()
                .WithName($"{configPath}__{nameof(ApplicationInsightConfig)}__{nameof(RoleName)}")
                .WithMessage("Please role name in ApplicationInsights with environment variable '{PropertyName}'")
                .WithSeverity(Severity.Warning)
                .Unless(x => string.IsNullOrEmpty(x.ConnectionString));

            RuleFor(x => x.RoleInstance)
                .NotEmpty()
                .WithName($"{configPath}__{nameof(ApplicationInsightConfig)}__{nameof(RoleInstance)}")
                .WithMessage("Please provide role instance in ApplicationInsights with environment variable '{PropertyName}'")
                .WithSeverity(Severity.Warning)
                .Unless(x => string.IsNullOrEmpty(x.ConnectionString));
        }
    }
}