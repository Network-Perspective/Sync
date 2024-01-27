using NetworkPerspective.Sync.Application.Domain.Networks.Filters;

namespace NetworkPerspective.Sync.Application.Domain.Networks
{
    /// <summary>
    /// Runtime config - retrieved from NP Core App in runtime
    /// </summary>
    public class NetworkConfig
    {
        public static readonly NetworkConfig Empty = new NetworkConfig(EmployeeFilter.Empty, CustomAttributesConfig.Empty);

        public EmployeeFilter EmailFilter { get; set; }
        public CustomAttributesConfig CustomAttributes { get; set; }

        public NetworkConfig(EmployeeFilter emailFilter, CustomAttributesConfig customAttributes)
        {
            EmailFilter = emailFilter;
            CustomAttributes = customAttributes;
        }
    }
}