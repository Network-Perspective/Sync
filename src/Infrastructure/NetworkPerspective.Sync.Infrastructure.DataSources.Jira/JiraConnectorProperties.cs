using System.Collections.Generic;

using FluentValidation;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira;

public class JiraConnectorProperties(IDictionary<string, string> props) : ConnectorProperties(props)
{
    public override bool SyncGroups { get; set; } = true;
    public override bool UseUserToken { get; set; } = true;

    public class Validator : AbstractValidator<JiraConnectorProperties>
    {
        public Validator()
        {
            RuleFor(x => x.UseUserToken)
                .Must(x => x == true)
                .WithMessage("For this connector only user tokens are supported");
        }
    }
}