using System.Collections.Generic;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira;

public class JiraConnectorProperties(IDictionary<string, string> props) : ConnectorProperties(props)
{
    public override bool SyncGroups { get; set; } = true;
    public override bool UseUserToken { get; set; } = true;
}