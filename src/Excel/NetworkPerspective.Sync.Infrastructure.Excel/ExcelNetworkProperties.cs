using NetworkPerspective.Sync.Application.Domain.Networks;

namespace NetworkPerspective.Sync.Infrastructure.Excel
{
    public class ExcelNetworkProperties : NetworkProperties
    {
        private new const bool DefaultSyncGroups = true;

        public ExcelNetworkProperties(Uri externalKeyVaultUri) : base(DefaultSyncGroups, externalKeyVaultUri)
        {
        }

        public ExcelNetworkProperties() : base(DefaultSyncGroups, null)
        { }
    }
}