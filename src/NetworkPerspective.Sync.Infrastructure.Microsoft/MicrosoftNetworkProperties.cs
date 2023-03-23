using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Application.Domain.Networks;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    public class MicrosoftNetworkProperties : NetworkProperties
    {
        private new const bool DefaultSyncGroups = true;

        public MicrosoftNetworkProperties(Uri externalKeyVaultUri) : base(DefaultSyncGroups, externalKeyVaultUri)
        { }

        public MicrosoftNetworkProperties() : base(DefaultSyncGroups, null)
        { }

        public override void Bind(IEnumerable<KeyValuePair<string, string>> properties)
        {
            base.Bind(properties);
        }

        public override IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            var props = new List<KeyValuePair<string, string>>
            { };

            props.AddRange(base.GetAll());

            return props;
        }
    }
}