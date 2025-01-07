using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google;

public class GoogleConnectorProperties : ConnectorProperties
{
    private new const bool DefaultSyncGroups = true;

    public string AdminEmail { get; set; }

    public GoogleConnectorProperties() : base(DefaultSyncGroups, DefaultSyncChannelsNames, DefaultUseUserToken, null)
    { }

    public override void Bind(IEnumerable<KeyValuePair<string, string>> properties)
    {
        base.Bind(properties);

        if (properties.Any(x => x.Key == nameof(AdminEmail)))
            AdminEmail = properties.Single(x => x.Key == nameof(AdminEmail)).Value;
    }

    public override IEnumerable<KeyValuePair<string, string>> GetAll()
    {
        var props = new List<KeyValuePair<string, string>>
        {
            new(nameof(AdminEmail), AdminEmail),
        };

        props.AddRange(base.GetAll());

        return props;
    }
}