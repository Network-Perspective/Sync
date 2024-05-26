using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Infrastructure.Slack.Client.Configs;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Pagination;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSlackClient(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            services.Configure<Resiliency>(configurationSection);

            services.AddTransient<CursorPaginationHandler>();
            services.AddScoped<ISlackClientFacadeFactory, SlackClientFacadeFactory>();

            services.AddScoped(sp => sp.GetRequiredService<ISlackClientFacadeFactory>().CreateUnauthorized());

            return services;
        }

    }
}
