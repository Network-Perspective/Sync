using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Infrastructure.Google
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGoogleDataSource(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<GoogleConfig>(configurationSection);

            services.AddSingleton<IDataSourceFactory, GoogleFacadeFactory>();
            return services;
        }
    }
}