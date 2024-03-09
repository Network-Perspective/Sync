using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Configs;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMicrosoft(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<Resiliency>(configurationSection.GetSection("Resiliency"));

            services.AddScoped<IMicrosoftAuthService, MicrosoftAuthService>();
            services.AddScoped<IMicrosoftClientFactory, MicrosoftClientFactory>();

            services.AddScoped(sp => sp.GetRequiredService<IMicrosoftClientFactory>().GetMicrosoftClientAsync().Result);

            services.AddScoped<IUsersClient, UsersClient>();
            services.AddScoped<IMailboxClient, MailboxClient>();
            services.AddScoped<ICalendarClient, CalendarClient>();
            services.AddScoped<IChannelsClient, ChannelsClient>();
            services.AddScoped<IChatsClient, ChatsClient>();

            services.AddScoped<IDataSource, MicrosoftFacade>();
            services.AddScoped<IAuthTester, AuthTester>();

            services.AddMemoryCache();

            return services;
        }
    }
}