using System;
using System.Net.Http;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Auth;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Pagination;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Configs;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Services;
using NetworkPerspective.Sync.Worker.Application;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJira(this IServiceCollection services, IConfigurationSection configurationSection, ConnectorType connectorType)
    {
        services.Configure<JiraConfig>(configurationSection);

        var apibaseUrl = configurationSection.GetValue<string>("BaseUrl");
        var authBaseUrl = configurationSection.GetValue<string>("Auth:BaseUrl");

        services
            .AddHttpClient(Consts.JiraApiHttpClientName, x =>
            {
                x.BaseAddress = new Uri(authBaseUrl);
            });

        services
            .AddHttpClient(Consts.JiraApiHttpClientWithTokenName, x =>
            {
                x.BaseAddress = new Uri(apibaseUrl);
            })
            .AddScopeAwareHttpHandler<AuthTokenHandler>();

        services.AddTransient<PaginationHandler>();

        services.AddScoped(sp =>
            {
                var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
                var httpClient = clientFactory.CreateClient(Consts.JiraApiHttpClientName);
                var jiraHttpClient = new JiraHttpClient(httpClient);
                return new JiraUnauthorizedFacade(jiraHttpClient) as IJiraUnauthorizedFacade;
            });

        services.AddScoped(sp =>
        {
            var paginationHandler = sp.GetRequiredService<PaginationHandler>();
            var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = clientFactory.CreateClient(Consts.JiraApiHttpClientWithTokenName);
            var jiraHttpClient = new JiraHttpClient(httpClient);
            return new JiraAuthorizedFacade(jiraHttpClient, paginationHandler) as IJiraAuthorizedFacade;
        });


        services.AddKeyedScoped<IAuthTester, AuthTester>(connectorType.GetKeyOf<IAuthTester>());
        services.AddKeyedScoped<IDataSource, JiraFacade>(connectorType.GetKeyOf<IDataSource>());
        services.AddKeyedScoped<IOAuthService, OAuthService>(connectorType.GetKeyOf<IOAuthService>());

        return services;
    }
}