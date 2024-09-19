using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira;


public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJira(this IServiceCollection services, IConfigurationSection configurationSection)
    {

        services.AddKeyedScoped<IAuthTester, AuthTester>((typeof(AuthTester).FullName));
        services.AddKeyedScoped<IDataSource, JiraFacade>(typeof(JiraFacade).FullName);

        return services;
    }
}