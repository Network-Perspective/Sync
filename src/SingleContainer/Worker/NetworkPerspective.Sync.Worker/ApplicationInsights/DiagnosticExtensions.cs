using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.Worker.ApplicationInsights;

public static class DiagnosticExtensions
{
    public static IServiceCollection LogApplicationVersion(this IServiceCollection services)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        var logger = services.BuildServiceProvider().GetRequiredService<ILogger<Program>>();
        logger.LogInformation("Application starting. Version: {Version}", version);

        return services;
    }
}