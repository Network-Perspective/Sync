﻿using System;
using System.Collections.Generic;

using Microsoft.Extensions.Configuration;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

public class ConnectorProperties
{
    public virtual bool SyncEmployees { get; set; } = false;
    public virtual bool SyncHashedEmployees { get; set; } = false;
    public virtual bool SyncGroups { get; set; } = false;
    public virtual bool SyncInteractions { get; set; } = false;
    public virtual bool SyncChannelsNames { get; set; } = false;
    public virtual bool UseUserToken { get; set; } = false;
    public virtual Uri ExternalKeyVaultUri { get; set; } = null;


    public ConnectorProperties(IDictionary<string, string> props)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(props)
            .Build();

        config.Bind(this);
    }
}