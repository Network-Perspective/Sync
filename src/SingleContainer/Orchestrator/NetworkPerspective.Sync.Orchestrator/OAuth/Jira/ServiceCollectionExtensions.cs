using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NetworkPerspective.Sync.Orchestrator.OAuth.Jira;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJiraAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JiraConfig>(configuration);

        services.AddScoped<IJiraAuthService, JiraAuthService>();

        return services;
    }
}