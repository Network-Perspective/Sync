using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira;

public class JiraNetworkProperties : ConnectorProperties
{
    private new const bool DefaultSyncGroups = true;
    private const bool DefaultSyncGroupAccess = false;

    public bool SyncGroupAccess { get; set; } = DefaultSyncGroupAccess;

    public JiraNetworkProperties()
    { }

    public JiraNetworkProperties(bool syncGroupAccess, Uri externalKeyVaultUri)
        : base(DefaultSyncGroups, false, externalKeyVaultUri)
    {
        SyncGroupAccess = syncGroupAccess;
    }

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