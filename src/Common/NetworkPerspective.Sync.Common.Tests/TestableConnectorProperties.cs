using System.Collections.Generic;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Common.Tests;

public class TestableConnectorProperties(IDictionary<string, string> keyValuePairs) : ConnectorProperties(keyValuePairs)
{
    public override bool SyncEmployees { get; set; } = true;
    public string StringProp { get; set; } = string.Empty;
    public int IntProp { get; set; } = 0;
    public bool BoolProp { get; set; } = false;
}