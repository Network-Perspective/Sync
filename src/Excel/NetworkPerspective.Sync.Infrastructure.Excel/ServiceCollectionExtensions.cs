using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Excel.Services;

namespace NetworkPerspective.Sync.Infrastructure.Excel
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExcel(this IServiceCollection services)
        {
            // scope data source to single request
            services.AddScoped<IDataSourceFactory, ExcelDataSourceFactory>();

            // dummy scheduler and auth tester
            services.AddTransient<ISyncScheduler, DummySyncScheduler>();
            services.AddTransient<IAuthTester, DummyAuthTester>();

            return services;
        }
    }
}