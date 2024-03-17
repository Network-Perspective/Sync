using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Domain.Networks;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    public class MicrosoftNetworkProperties : NetworkProperties
    {
        private new const bool DefaultSyncGroups = true;

        public bool SyncMsTeams { get; set; } = true;
        public bool SyncChats { get; set; } = true;
        //public bool SyncChannelsNames { get; set; } = false;
        //public bool SyncGroupAccess { get; set; } = false;

        public MicrosoftNetworkProperties(bool syncMsTeams, bool syncChats,/* bool syncChannelsNames, bool syncGroupAccess, */ Uri externalKeyVaultUri) : base(DefaultSyncGroups, externalKeyVaultUri)
        {
            SyncMsTeams = syncMsTeams;
            SyncChats = syncChats;
            //SyncChannelsNames = syncChannelsNames;
            //SyncGroupAccess = syncGroupAccess;
        }

        public MicrosoftNetworkProperties() : base(DefaultSyncGroups, null)
        { }

        public override void Bind(IEnumerable<KeyValuePair<string, string>> properties)
        {
            if (properties.Any(x => x.Key == nameof(SyncMsTeams)))
                SyncMsTeams = bool.Parse(properties.Single(x => x.Key == nameof(SyncMsTeams)).Value);

            if (properties.Any(x => x.Key == nameof(SyncChats)))
                SyncChats = bool.Parse(properties.Single(x => x.Key == nameof(SyncChats)).Value);

            //if (properties.Any(x => x.Key == nameof(SyncChannelsNames)))
            //    SyncChannelsNames = bool.Parse(properties.Single(x => x.Key == nameof(SyncChannelsNames)).Value);

            //if (properties.Any(x => x.Key == nameof(SyncGroupAccess)))
            //    SyncGroupAccess = bool.Parse(properties.Single(x => x.Key == nameof(SyncGroupAccess)).Value);

            base.Bind(properties);
        }

        public override IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            var props = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(nameof(SyncMsTeams), SyncMsTeams.ToString()),
                new KeyValuePair<string, string>(nameof(SyncChats), SyncChats.ToString()),
                //new KeyValuePair<string, string>(nameof(SyncChannelsNames), SyncChannelsNames.ToString()),
                //new KeyValuePair<string, string>(nameof(SyncGroupAccess), SyncGroupAccess.ToString()),
            };

            props.AddRange(base.GetAll());

            return props;
        }
    }
}