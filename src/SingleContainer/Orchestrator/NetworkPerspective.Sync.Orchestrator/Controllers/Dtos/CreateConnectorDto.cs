using System;
using System.Collections.Generic;

using FluentValidation;

namespace NetworkPerspective.Sync.Orchestrator.Controllers.Dtos;

public class CreateConnectorDto
{
    public Guid Id { get; set; }
    public Guid NetworkId { get; set; }
    public Guid WorkerId { get; set; }
    public string Type { get; set; }
    public string AccessToken { get; set; }
    public IEnumerable<ConnectorPropertyDto> Properties { get; set; }

    public class Validator : AbstractValidator<CreateConnectorDto>
    {
        public Validator()
        {
            RuleFor(x => x.Id)
                .NotEmpty();

            RuleFor(x => x.NetworkId)
                .NotEmpty();

            RuleFor(x => x.WorkerId)
                .NotEmpty();

            RuleFor(x => x.Type)
                .Matches(@"^[a-zA-Z0-9]+$")
                .WithMessage($"{nameof(Type)} may contain only alpha-numeric characters");
        }
    }
}