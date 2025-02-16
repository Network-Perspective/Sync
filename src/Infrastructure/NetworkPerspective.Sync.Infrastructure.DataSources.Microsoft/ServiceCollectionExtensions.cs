using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Configs;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services.Auths;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMicrosoft(this IServiceCollection services, IConfiguration configuration, ConnectorType connectorType)
    {
        services.Configure<ResiliencyConfig>(configuration.GetSection("Resiliency"));
        services.Configure<AuthConfig>(configuration.GetSection("Auth"));

        services.AddTransient<ICapabilityTester>(x =>
        {
            var vault = x.GetRequiredService<IVault>();
            var logger = x.GetRequiredService<ILogger<CapabilityTester>>();
            return new CapabilityTester(connectorType, vault, logger);
        });

        services.AddScoped<IMicrosoftClientFactory, MicrosoftClientFactory>();

        services.AddScoped(sp => sp.GetRequiredService<IMicrosoftClientFactory>().GetMicrosoftClientAsync().Result);

        services.AddScoped<IUsersClient, UsersClient>();
        services.AddScoped<IMailboxClient, MailboxClient>();
        services.AddScoped<ICalendarClient, CalendarClient>();
        services.AddScoped<IChannelsClient, ChannelsClient>();
        services.AddScoped<IChatsClient, ChatsClient>();
        services.AddScoped<IConfidentialClientAppProvider, ConfidentialClientAppProvider>();
        services.AddScoped<IUserTokenCacheVaultBinder, UserTokenCacheVaultBinder>();
        services.AddScoped<CustomAuthenticationProvider>();

        services.AddKeyedScoped<IAuthTester, AuthTester>(connectorType.GetKeyOf<IAuthTester>());
        services.AddKeyedScoped<IDataSource, MicrosoftFacade>(connectorType.GetKeyOf<IDataSource>());

        services.AddScoped<UserOAuthService>();
        services.AddScoped<AdminConsentOAuthService>();
        services.AddKeyedScoped<IOAuthService>(connectorType.GetKeyOf<IOAuthService>(), (sp, key) =>
        {
            var connectorContextAccessor = sp.GetRequiredService<IConnectorContextAccessor>();
            var properties = new ConnectorProperties(connectorContextAccessor.Context.Properties);

            return properties.UseUserToken
                ? sp.GetRequiredService<UserOAuthService>()
                : sp.GetRequiredService<AdminConsentOAuthService>();
        });

        services.AddMemoryCache();

        return services;
    }
}