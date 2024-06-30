using System;
using System.Collections.Generic;
using System.Security;

using NetworkPerspective.Sync.Utils.Models;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class SyncContext
{
    public Guid ConnectorId { get; set; }
    public Guid NetworkId { get; set; }
    public TimeRange TimeRange { get; set; }
    public SecureString AccessToken { get; set; }
    public IDictionary<string, string> NetworkProperties { get; set; }
}