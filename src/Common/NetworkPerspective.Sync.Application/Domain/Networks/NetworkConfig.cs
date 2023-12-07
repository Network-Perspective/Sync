namespace NetworkPerspective.Sync.Application.Domain.Networks
{
    /// <summary>
    /// Runtime config - retrieved from NP Core App in runtime
    /// </summary>
    public class NetworkConfig
    {
        public static readonly NetworkConfig Empty = new NetworkConfig(EmailFilter.Empty, CustomAttributesConfig.Empty);

        public EmailFilter EmailFilter { get; set; }
        public CustomAttributesConfig CustomAttributes { get; set; }

        public NetworkConfig(EmailFilter emailFilter, CustomAttributesConfig customAttributes)
        {
            EmailFilter = emailFilter;
            CustomAttributes = customAttributes;
        }
    }
}