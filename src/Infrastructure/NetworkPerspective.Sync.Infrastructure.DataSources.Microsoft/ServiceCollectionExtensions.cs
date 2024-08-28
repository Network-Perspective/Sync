using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Configs;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMicrosoft(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        services.Configure<Resiliency>(configurationSection.GetSection("Resiliency"));

        services.AddScoped<IMicrosoftClientFactory, MicrosoftClientFactory>();

        services.AddScoped(sp => sp.GetRequiredService<IMicrosoftClientFactory>().GetMicrosoftClientAsync().Result);

        services.AddScoped<IUsersClient, UsersClient>();
        services.AddScoped<IMailboxClient, MailboxClient>();
        services.AddScoped<ICalendarClient, CalendarClient>();
        services.AddScoped<IChannelsClient, ChannelsClient>();
        services.AddScoped<IChatsClient, ChatsClient>();

        services.AddKeyedScoped<IAuthTester, AuthTester>(typeof(AuthTester).FullName);
        services.AddKeyedScoped<IDataSource, MicrosoftFacade>(typeof(MicrosoftFacade).FullName);

        services.AddMemoryCache();

        return services;
    }
}