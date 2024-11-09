using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Configs;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMicrosoft(this IServiceCollection services, IConfigurationSection configurationSection, ConnectorType connectorType)
    {
        services.Configure<Resiliency>(configurationSection.GetSection("Resiliency"));

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

        services.AddKeyedScoped<IAuthTester, AuthTester>(connectorType.GetKeyOf<IAuthTester>());
        services.AddKeyedScoped<IDataSource, MicrosoftFacade>(connectorType.GetKeyOf<IDataSource>());
        services.AddKeyedScoped<IOAuthService, OAuthService>(connectorType.GetKeyOf<IOAuthService>());

        services.AddMemoryCache();

        return services;
    }
}