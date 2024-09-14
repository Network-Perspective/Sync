using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Excel;

public static class ServiceCollectionExtensions
{
    private const string SyncConstraintsConfigSection = "SyncConstraints";
    public static IServiceCollection AddExcel(this IServiceCollection services, IConfigurationSection config)
    {
        services.AddKeyedScoped<IDataSource, ExcelFacade>(typeof(ExcelFacade).FullName);

        services.Configure<ExcelSyncConstraints>(config.GetSection(SyncConstraintsConfigSection));
        return services;
    }
}