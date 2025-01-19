using System.Collections.Generic;

using FluentValidation;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google;

public class GoogleConnectorProperties(IDictionary<string, string> props) : ConnectorProperties(props)
{
    public override bool SyncGroups { get; set; } = true;
    public string AdminEmail { get; set; }

    public class Validator : AbstractValidator<GoogleConnectorProperties>
    {
        public Validator()
        {
            RuleFor(x => x.AdminEmail)
                .NotEmpty();

            RuleFor(x => x.SyncChannelsNames)
                .Must(x => x == false)
                .Unless(x => x.SyncGroups == true)
                .WithMessage($"To synchronize Channel Names {nameof(SyncGroups)} needs to be enabled");

            RuleFor(x => x.SyncInteractions)
                .Must(x => x == false)
                .Unless(x => x.UseUserToken == false)
                .WithMessage($"To synchronize Interactions {nameof(UseUserToken)} needs to be disabled");
        }
    }
}