using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Options;

namespace NetworkPerspective.Sync.Orchestrator.ApplicationInsights;

public class CloudContextTelemetryInitializer(IOptions<ApplicationInsightConfig> config) : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        if (!string.IsNullOrEmpty(config.Value.RoleName))
            telemetry.Context.Cloud.RoleName = config.Value.RoleName;

        if (!string.IsNullOrEmpty(config.Value.RoleInstance))
            telemetry.Context.Cloud.RoleInstance = config.Value.RoleInstance;
    }
}