using System;

using FluentValidation;

namespace NetworkPerspective.Sync.Contract.V1.Impl;

public sealed class OrchestratorHubClientConfig
{
    public string BaseUrl { get; set; }
    public Resiliency Resiliency { get; set; } = new Resiliency();

    public class Validator : AbstractValidator<OrchestratorHubClientConfig>
    {
        public Validator()
        {
            RuleFor(x => x.BaseUrl)
                .NotEmpty()
                .WithMessage($"Orchestrator '{nameof(BaseUrl)}' cannot be null or empty");
        }
    }
}

public sealed class Resiliency
{
    public TimeSpan[] Retries { get; set; } = [];
}