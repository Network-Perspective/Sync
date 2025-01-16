using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Excel;

public class ExcelConnectorProperties(IDictionary<string, string> props) : ConnectorProperties(props)
{
    public override bool SyncGroups { get; set; } = true;
}