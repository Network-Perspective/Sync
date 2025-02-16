using System;

using FluentValidation;

namespace NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

public class CreateWorkerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Secret { get; set; }

    public class Validator : AbstractValidator<CreateWorkerDto>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.Name)
                .Matches(@"^[a-zA-Z0-9-_]+$")
                .WithMessage($"{nameof(Name)} may contain only alpha-numeric, '-' and '_' characters");
        }
    }
}