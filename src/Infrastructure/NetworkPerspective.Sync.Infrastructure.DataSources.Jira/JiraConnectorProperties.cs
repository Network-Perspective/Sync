using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira;

public class JiraConnectorProperties : ConnectorProperties
{
    private new const bool DefaultSyncGroups = true;
    private new const bool DefaultUseUserToken = true;

    private const bool DefaultSyncGroupAccess = false;

    public bool SyncGroupAccess { get; set; } = DefaultSyncGroupAccess;

    public JiraConnectorProperties() : base(DefaultSyncGroups, DefaultSyncChannelsNames, DefaultUseUserToken, null)
    { }

    public override void Bind(IEnumerable<KeyValuePair<string, string>> properties)
    {
        if (properties.Any(x => x.Key == nameof(SyncGroupAccess)))
            SyncGroupAccess = bool.Parse(properties.Single(x => x.Key == nameof(SyncGroupAccess)).Value);

        base.Bind(properties);
    }

    public override IEnumerable<KeyValuePair<string, string>> GetAll()
    {
        var props = new List<KeyValuePair<string, string>>
        {
            new(nameof(SyncGroupAccess), SyncGroupAccess.ToString()),
        };

        props.AddRange(base.GetAll());

        return props;
    }
}