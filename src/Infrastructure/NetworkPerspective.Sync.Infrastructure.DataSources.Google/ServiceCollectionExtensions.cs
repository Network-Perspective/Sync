using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Criterias;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGoogle(this IServiceCollection services, IConfigurationSection configurationSection, ConnectorType connectorType)
    {
        services.Configure<GoogleConfig>(configurationSection);

        services.AddTransient<IRetryPolicyProvider, RetryPolicyProvider>();

        services.AddScoped<ICredentialsProvider, CredentialsProvider>();
        services.AddScoped<ICriteria, NonServiceUserCriteria>();

        services.AddScoped<IMailboxClient, MailboxClient>();
        services.AddScoped<ICalendarClient, CalendarClient>();
        services.AddScoped<IUsersClient, UsersClient>();
        services.AddScoped<IUserCalendarTimeZoneReader, UserCalendarTimeZoneReader>();

        services.AddKeyedScoped<IAuthTester, AuthTester>(connectorType.GetKeyOf<IAuthTester>());
        services.AddKeyedScoped<IDataSource, GoogleFacade>(connectorType.GetKeyOf<IDataSource>());

        return services;
    }
}