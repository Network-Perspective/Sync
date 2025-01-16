using System.Collections.Generic;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack;

public class SlackConnectorProperties(IDictionary<string, string> props) : ConnectorProperties(props)
{
    public bool AutoJoinChannels { get; set; } = true;
    public bool UsesAdminPrivileges { get; set; } = false;
}