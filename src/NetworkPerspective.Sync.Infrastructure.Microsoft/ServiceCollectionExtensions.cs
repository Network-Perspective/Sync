using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Configs;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMicrosoft(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<Resiliency>(configurationSection.GetSection("Resiliency"));

            services.AddTransient<IMicrosoftAuthService, MicrosoftAuthService>();
            services.AddTransient<IMicrosoftClientFactory, MicrosoftClientFactory>();
            services.AddTransient<IDataSourceFactory, MicrosoftFacadeFactory>();

            services.AddMemoryCache();

            return services;
        }
    }
}