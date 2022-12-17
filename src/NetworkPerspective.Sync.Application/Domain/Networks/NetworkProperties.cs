using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkPerspective.Sync.Application.Domain.Networks
{
    public class NetworkProperties
    {
        protected const bool DefaultSyncGroups = false;
        protected const bool DefaultUseDurableIntractionsCache = false;

        public bool SyncGroups { get; private set; } = DefaultSyncGroups;
        public Uri ExternalKeyVaultUri { get; private set; } = null;
        public bool UseDurableInteractionsCache { get; private set; } = DefaultUseDurableIntractionsCache;

        public NetworkProperties()
        { }

        public NetworkProperties(bool syncGroups, Uri externalKeyVaultUri, bool useDurableInteractionsCache)
        {
            SyncGroups = syncGroups;
            ExternalKeyVaultUri = externalKeyVaultUri;
            UseDurableInteractionsCache = useDurableInteractionsCache;
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

            if (properties.Any(x => x.Key == nameof(UseDurableInteractionsCache)))
                UseDurableInteractionsCache = bool.Parse(properties.Single(x => x.Key == nameof(UseDurableInteractionsCache)).Value);
        }

        public virtual IEnumerable<KeyValuePair<string, string>> GetAll()
        {
            var result = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>(nameof(SyncGroups), SyncGroups.ToString()),
                new KeyValuePair<string, string>(nameof(UseDurableInteractionsCache), UseDurableInteractionsCache.ToString())
            };

            if (ExternalKeyVaultUri is not null)
                result.Add(new KeyValuePair<string, string>(nameof(ExternalKeyVaultUri), ExternalKeyVaultUri?.ToString()));

            return result;
        }
    }
}