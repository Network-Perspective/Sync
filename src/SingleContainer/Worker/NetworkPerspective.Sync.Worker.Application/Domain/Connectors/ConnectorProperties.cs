using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

public class ConnectorProperties
{
    protected const bool DefaultSyncGroups = false;
    protected const bool DefaultSyncChannelsNames = false;
    protected const bool DefaultUseUserToken = false;

    public bool SyncGroups { get; private set; } = DefaultSyncGroups;
    public bool SyncChannelsNames { get; private set; } = DefaultSyncChannelsNames;
    public bool UseUserToken { get; private set; } = false;
    public Uri ExternalKeyVaultUri { get; private set; } = null;

    public ConnectorProperties()
    { }

    public ConnectorProperties(bool syncGroups, bool syncChannelsNames, bool useUserToken, Uri externalKeyVaultUri)
    {
        SyncGroups = syncGroups;
        SyncChannelsNames = syncChannelsNames;
        UseUserToken = useUserToken;
        ExternalKeyVaultUri = externalKeyVaultUri;
    }

    public static TProperties Create<TProperties>(IEnumerable<KeyValuePair<string, string>> properties) where TProperties : ConnectorProperties, new()
    {
        var networkProperties = new TProperties();
        networkProperties.Bind(properties);
        return networkProperties;
    }

    public virtual void Bind(IEnumerable<KeyValuePair<string, string>> properties)
    {
        if (properties.Any(x => x.Key == nameof(SyncGroups)))
            SyncGroups = bool.Parse(properties.Single(x => x.Key == nameof(SyncGroups)).Value);

        if (properties.Any(x => x.Key == nameof(SyncChannelsNames)))
            SyncChannelsNames = bool.Parse(properties.Single(x => x.Key == nameof(SyncChannelsNames)).Value);

        if (properties.Any(x => x.Key == nameof(UseUserToken)))
            UseUserToken = bool.Parse(properties.Single(x => x.Key == nameof(UseUserToken)).Value);

        if (properties.Any(x => x.Key == nameof(ExternalKeyVaultUri)))
            ExternalKeyVaultUri = new Uri(properties.Single(x => x.Key == nameof(ExternalKeyVaultUri)).Value);
    }

    public virtual IEnumerable<KeyValuePair<string, string>> GetAll()
    {
        var result = new List<KeyValuePair<string, string>>
        {
            new(nameof(SyncGroups), SyncGroups.ToString()),
            new(nameof(SyncChannelsNames), SyncChannelsNames.ToString()),
            new(nameof(UseUserToken), UseUserToken.ToString()),
        };

        if (ExternalKeyVaultUri is not null)
            result.Add(new KeyValuePair<string, string>(nameof(ExternalKeyVaultUri), ExternalKeyVaultUri?.ToString()));

        return result;
    }
}