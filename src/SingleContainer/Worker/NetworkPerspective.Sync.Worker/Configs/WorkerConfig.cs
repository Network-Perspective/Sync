﻿using System.Linq;

using FluentValidation;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Worker.ApplicationInsights;

namespace NetworkPerspective.Sync.Worker.Configs;

public class WorkerConfig
{
    public ApplicationInsightConfig ApplicationInsights { get; set; }
    public InfrastructureConfig Infrastructure { get; set; }

    public class Validator : AbstractValidator<WorkerConfig>, IValidateOptions<WorkerConfig>
    {
        private readonly ILogger<Validator> _logger;

        public Validator(ILogger<Validator> logger)
        {
            RuleFor(x => x.Infrastructure)
                .SetValidator(x => new InfrastructureConfig.Validator($"{nameof(Infrastructure)}"));

            RuleFor(x => x.ApplicationInsights)
                .SetValidator(x => new ApplicationInsightConfig.Validator($"{nameof(ApplicationInsights)}"));

            _logger = logger;
        }

        public ValidateOptionsResult Validate(string name, WorkerConfig config)
        {
            var validateResult = Validate(config);

            var errors = validateResult.Errors.Where(x => x.Severity == Severity.Error);
            var warnings = validateResult.Errors.Where(x => x.Severity == Severity.Warning);

            var isValid = !errors.Any();

            foreach (var warning in warnings)
                _logger.LogWarning(warning.ErrorMessage);

            foreach (var error in errors)
                _logger.LogCritical(error.ErrorMessage);

            return isValid
                ? ValidateOptionsResult.Success
                : ValidateOptionsResult.Fail(validateResult.Errors.Select(x => x.ErrorMessage));
        }
    }
}