using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Domain.Networks;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    public class MicrosoftNetworkProperties : NetworkProperties
    {
        private new const bool DefaultSyncGroups = true;

        public bool SyncMsTeams { get; set; }

        public MicrosoftNetworkProperties(bool syncMsTeams, Uri externalKeyVaultUri) : base(DefaultSyncGroups, externalKeyVaultUri)
        {
            SyncMsTeams = syncMsTeams;
        }

        public MicrosoftNetworkProperties() : base(DefaultSyncGroups, null)
        { }

        public override void Bind(IEnumerable<KeyValuePair<string, string>> properties)
        {
            if (properties.Any(x => x.Key == nameof(SyncMsTeams)))
                SyncMsTeams = bool.Parse(properties.Single(x => x.Key == nameof(SyncMsTeams)).Value);

            base.Bind(properties);
        }

        public override IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            var props = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(nameof(SyncMsTeams), SyncMsTeams.ToString()),
            };

            props.AddRange(base.GetAll());

            return props;
        }
    }
}