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
                .WithName($"{configPath}:{nameof(ApplicationInsightConfig)}:{nameof(ConnectionString)}")
                .WithSeverity(Severity.Warning);

            RuleFor(x => x.RoleName)
                .NotEmpty()
                .WithName($"{configPath}:{nameof(ApplicationInsightConfig)}:{nameof(RoleName)}")
                .WithSeverity(Severity.Warning)
                .Unless(x => string.IsNullOrEmpty(x.ConnectionString));

            RuleFor(x => x.RoleInstance)
                .NotEmpty()
                .WithName($"{configPath}:{nameof(ApplicationInsightConfig)}:{nameof(RoleInstance)}")
                .WithSeverity(Severity.Warning)
                .Unless(x => string.IsNullOrEmpty(x.ConnectionString));
        }
    }
}