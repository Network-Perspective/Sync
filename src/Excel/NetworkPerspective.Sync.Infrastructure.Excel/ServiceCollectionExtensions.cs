using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Infrastructure.Excel.Services;

namespace NetworkPerspective.Sync.Infrastructure.Excel
{
    public static class ServiceCollectionExtensions
    {
        private const string SyncConstraintsConfigSection = "SyncConstraints";

        public static IServiceCollection AddExcel(this IServiceCollection services, IConfigurationSection config)
        {
            // scope data source to single request
            services.AddScoped<IDataSource, ExcelDataSource>();

            // add constraints configuration
            services.Configure<ExcelSyncConstraints>(config.GetSection(SyncConstraintsConfigSection));

            return services;
        }
    }
}