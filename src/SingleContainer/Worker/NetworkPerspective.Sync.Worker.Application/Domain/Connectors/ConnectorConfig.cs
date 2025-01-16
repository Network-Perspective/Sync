using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

/// <summary>
/// Runtime config - retrieved from NP Core App in runtime
/// </summary>
public class ConnectorConfig(EmployeeFilter emailFilter, CustomAttributesConfig customAttributes)
{
    public static readonly ConnectorConfig Empty = new(EmployeeFilter.Empty, CustomAttributesConfig.Empty);

    public EmployeeFilter EmailFilter { get; set; } = emailFilter;
    public CustomAttributesConfig CustomAttributes { get; set; } = customAttributes;
}