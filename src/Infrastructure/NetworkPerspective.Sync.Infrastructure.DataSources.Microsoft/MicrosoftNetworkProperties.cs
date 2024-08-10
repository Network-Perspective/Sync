using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft
{
    public class MicrosoftNetworkProperties : ConnectorProperties
    {
        private new const bool DefaultSyncGroups = true;
        private const bool DefaultSyncGroupAccess = false;

        public bool SyncMsTeams { get; set; } = true;
        public bool SyncChats { get; set; } = true;
        public bool SyncGroupAccess { get; set; } = DefaultSyncGroupAccess;

        public MicrosoftNetworkProperties(bool syncMsTeams, bool syncChats, bool syncChannelsNames, bool syncGroupAccess, Uri externalKeyVaultUri) : base(DefaultSyncGroups, syncChannelsNames, externalKeyVaultUri)
        {
            SyncMsTeams = syncMsTeams;
            SyncChats = syncChats;
            SyncGroupAccess = syncGroupAccess;
        }

        public MicrosoftNetworkProperties() : base(DefaultSyncGroups, DefaultSyncChannelsNames, null)
        { }

        public override void Bind(IEnumerable<KeyValuePair<string, string>> properties)
        {
            if (properties.Any(x => x.Key == nameof(SyncMsTeams)))
                SyncMsTeams = bool.Parse(properties.Single(x => x.Key == nameof(SyncMsTeams)).Value);

            if (properties.Any(x => x.Key == nameof(SyncChats)))
                SyncChats = bool.Parse(properties.Single(x => x.Key == nameof(SyncChats)).Value);

            if (properties.Any(x => x.Key == nameof(SyncGroupAccess)))
                SyncGroupAccess = bool.Parse(properties.Single(x => x.Key == nameof(SyncGroupAccess)).Value);

            base.Bind(properties);
        }

        public override IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            var props = new List<KeyValuePair<string, string>>
            {
                new(nameof(SyncMsTeams), SyncMsTeams.ToString()),
                new(nameof(SyncChats), SyncChats.ToString()),
                new(nameof(SyncGroupAccess), SyncGroupAccess.ToString()),
            };

            props.AddRange(base.GetAll());

            return props;
        }
    }
}