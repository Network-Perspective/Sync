using Microsoft.ApplicationInsights.AspNetCore.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.ApplicationInsights;

public class ApplicationInsightConfig : ApplicationInsightsServiceOptions
{
    public string RoleName { get; set; }
    public string RoleInstance { get; set; }
}