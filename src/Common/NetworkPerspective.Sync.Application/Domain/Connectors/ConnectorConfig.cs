using NetworkPerspective.Sync.Application.Domain.Networks.Filters;

namespace NetworkPerspective.Sync.Application.Domain.Connectors
{
    /// <summary>
    /// Runtime config - retrieved from NP Core App in runtime
    /// </summary>
    public class ConnectorConfig
    {
        public static readonly ConnectorConfig Empty = new ConnectorConfig(EmployeeFilter.Empty, CustomAttributesConfig.Empty);

        public EmployeeFilter EmailFilter { get; set; }
        public CustomAttributesConfig CustomAttributes { get; set; }

        public ConnectorConfig(EmployeeFilter emailFilter, CustomAttributesConfig customAttributes)
        {
            EmailFilter = emailFilter;
            CustomAttributes = customAttributes;
        }
    }
}