using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Orchestrator.OAuth.Jira.Client;

namespace NetworkPerspective.Sync.Orchestrator.OAuth.Jira;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJiraAuth(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JiraConfig>(configuration);

        services.AddMemoryCache();

        services.AddScoped<IJiraClient, JiraClient>();
        services.AddScoped<IJiraAuthService, JiraAuthService>();

        return services;
    }
}