using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Domain.Networks;

namespace NetworkPerspective.Sync.Infrastructure.Slack
{
    public class SlackNetworkProperties : NetworkProperties
    {
        public bool AutoJoinChannels { get; private set; } = true;
        public bool UsesAdminPrivileges { get; private set; } = false;

        public SlackNetworkProperties() : base(DefaultSyncGroups, null)
        { }

        public SlackNetworkProperties(bool autoJoinChannels, bool requireAdminPrivileges, bool syncChannelsNames, Uri externalKeyVaultUri) : base(syncChannelsNames, externalKeyVaultUri)
        {
            AutoJoinChannels = autoJoinChannels;
            UsesAdminPrivileges = requireAdminPrivileges;
        }

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
                new KeyValuePair<string, string>(nameof(AutoJoinChannels), AutoJoinChannels.ToString()),
                new KeyValuePair<string, string>(nameof(UsesAdminPrivileges), UsesAdminPrivileges.ToString()),
            };

            props.AddRange(base.GetAll());

            return props;
        }
    }
}