using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Services;

namespace NetworkPerspective.Sync.Infrastructure.Google
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddGoogleDataSource(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<GoogleConfig>(configurationSection);
            services.AddTransient<IAuthTester, AuthTester>();
            services.AddSingleton<IDataSourceFactory, GoogleFacadeFactory>();
            return services;
        }
    }
}