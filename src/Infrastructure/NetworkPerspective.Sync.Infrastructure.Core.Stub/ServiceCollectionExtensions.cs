using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Worker.Application.Infrastructure.Core;

namespace NetworkPerspective.Sync.Infrastructure.Core.Stub
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddNetworkPerspectiveCoreStub(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            var config = new NetworkPerspectiveCoreConfig();
            configurationSection.Bind(config);
            services.Configure<NetworkPerspectiveCoreConfig>(configurationSection);

            const string DirectoryName = "Data";

            services.AddSingleton(new FileDataWriter(DirectoryName));
            services.AddTransient<INetworkPerspectiveCore, NetworkPerspectiveCoreStub>();

            return services;
        }
    }
}