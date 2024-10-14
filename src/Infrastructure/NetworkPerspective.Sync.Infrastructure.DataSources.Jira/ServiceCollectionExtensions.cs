using System;
using System.Net.Http;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Auth;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.HttpClients;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Pagination;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Services;
using NetworkPerspective.Sync.Worker.Application;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJira(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        var authBaseUrl = configurationSection.GetValue<string>("AuthBaseUrl");
        var apibaseUrl = configurationSection.GetValue<string>("ApiBaseUrl");

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


        services.AddKeyedScoped<IAuthTester, AuthTester>((typeof(AuthTester).FullName));
        services.AddKeyedScoped<IDataSource, JiraFacade>(typeof(JiraFacade).FullName);

        return services;
    }
}