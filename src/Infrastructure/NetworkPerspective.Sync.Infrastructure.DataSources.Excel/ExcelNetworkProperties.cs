using NetworkPerspective.Sync.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Excel
{
    public class ExcelNetworkProperties : ConnectorProperties
    {
        private new const bool DefaultSyncGroups = true;

        public ExcelNetworkProperties(Uri externalKeyVaultUri) : base(DefaultSyncGroups, DefaultSyncChannelsNames, externalKeyVaultUri)
        {
        }

        public ExcelNetworkProperties() : base(DefaultSyncGroups, DefaultSyncChannelsNames, null)
        { }
    }
}