using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Domain.Networks;

namespace NetworkPerspective.Sync.Infrastructure.Slack
{
    public class SlackNetworkProperties : NetworkProperties
    {
        public bool AutoJoinChannels { get; private set; } = true;

        public SlackNetworkProperties() : base(DefaultSyncGroups, null, DefaultUseDurableIntractionsCache)
        { }

        public SlackNetworkProperties(bool autoJoinChannels, bool syncChannelsNames, Uri externalKeyVaultUri, bool useDurableInteractionsCache) : base(syncChannelsNames, externalKeyVaultUri, useDurableInteractionsCache)
        {
            AutoJoinChannels = autoJoinChannels;
        }

        public override void Bind(IEnumerable<KeyValuePair<string, string>> properties)
        {
            base.Bind(properties);

            if (properties.Any(x => x.Key == nameof(AutoJoinChannels)))
                AutoJoinChannels = bool.Parse(properties.Single(x => x.Key == nameof(AutoJoinChannels)).Value);
        }

        public override IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            var props = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(nameof(AutoJoinChannels), AutoJoinChannels.ToString()),
            };

            props.AddRange(base.GetAll());

            return props;
        }
    }
}