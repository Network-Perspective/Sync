using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkPerspective.Sync.Application.Domain.Networks
{
    public class NetworkProperties
    {
        protected const bool DefaultSyncGroups = false;

        public bool SyncGroups { get; private set; } = DefaultSyncGroups;

        public Uri ExternalKeyVaultUri { get; private set; } = null;

        public NetworkProperties()
        { }

        public NetworkProperties(bool syncGroups, Uri externalKeyVaultUri)
        {
            SyncGroups = syncGroups;
            ExternalKeyVaultUri = externalKeyVaultUri;
        }

        public static TProperties Create<TProperties>(IEnumerable<KeyValuePair<string, string>> properties) where TProperties : NetworkProperties, new()
        {
            var networkProperties = new TProperties();
            networkProperties.Bind(properties);
            return networkProperties;
        }

        public virtual void Bind(IEnumerable<KeyValuePair<string, string>> properties)
        {
            if (properties.Any(x => x.Key == nameof(SyncGroups)))
                SyncGroups = bool.Parse(properties.Single(x => x.Key == nameof(SyncGroups)).Value);

            if (properties.Any(x => x.Key == nameof(ExternalKeyVaultUri)))
                ExternalKeyVaultUri = new Uri(properties.Single(x => x.Key == nameof(ExternalKeyVaultUri)).Value);
        }

        public virtual IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            var result = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(nameof(SyncGroups), SyncGroups.ToString())
            };

            if (ExternalKeyVaultUri is not null)
                result.Add(new KeyValuePair<string, string>(nameof(ExternalKeyVaultUri), ExternalKeyVaultUri?.ToString()));

            return result;
        }
    }
}