using System.Collections.Generic;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft;

public class MicrosoftConnectorProperties(IDictionary<string, string> props) : ConnectorProperties(props)
{
    public override bool SyncGroups { get; set; } = true;

    public bool SyncMsTeams { get; set; } = true;
    public bool SyncChats { get; set; } = true;
    public bool SyncGroupAccess { get; set; } = false;
}