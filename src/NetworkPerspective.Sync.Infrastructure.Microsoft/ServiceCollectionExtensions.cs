using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMicrosoft(this IServiceCollection services, IConfigurationSection configSection)
        {
            services.AddSingleton<IDataSourceFactory, MicrosoftFacadeFactory>();
            return services;
        }
    }
}