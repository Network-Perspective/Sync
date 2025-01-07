using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Excel;

public class ExcelConnectorProperties : ConnectorProperties
{
    private new const bool DefaultSyncGroups = true;

    public ExcelConnectorProperties() : base(DefaultSyncGroups, DefaultSyncChannelsNames, DefaultUseUserToken, null)
    { }
}