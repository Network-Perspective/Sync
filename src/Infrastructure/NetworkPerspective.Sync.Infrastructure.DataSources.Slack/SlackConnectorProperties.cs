using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack;

public class SlackConnectorProperties : ConnectorProperties
{
    public bool AutoJoinChannels { get; private set; } = true;
    public bool UsesAdminPrivileges { get; private set; } = false;

    public SlackConnectorProperties() : base(DefaultSyncGroups, DefaultSyncChannelsNames, DefaultUseUserToken, null)
    { }

    public override void Bind(IEnumerable<KeyValuePair<string, string>> properties)
    {
        base.Bind(properties);

        if (properties.Any(x => x.Key == nameof(AutoJoinChannels)))
            AutoJoinChannels = bool.Parse(properties.Single(x => x.Key == nameof(AutoJoinChannels)).Value);

        if (properties.Any(x => x.Key == nameof(UsesAdminPrivileges)))
            UsesAdminPrivileges = bool.Parse(properties.Single(x => x.Key == nameof(UsesAdminPrivileges)).Value);
    }

    public override IEnumerable<KeyValuePair<string, string>> GetAll()
    {
        var props = new List<KeyValuePair<string, string>>
        {
            new(nameof(AutoJoinChannels), AutoJoinChannels.ToString()),
            new(nameof(UsesAdminPrivileges), UsesAdminPrivileges.ToString()),
        };

        props.AddRange(base.GetAll());

        return props;
    }
}