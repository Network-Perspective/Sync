using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Excel;

public static class ServiceCollectionExtensions
{
    private const string SyncConstraintsConfigSection = "SyncConstraints";
    public static IServiceCollection AddExcel(this IServiceCollection services, IConfigurationSection config, ConnectorType connectorType)
    {
        services.AddKeyedScoped<IDataSource, ExcelFacade>(connectorType.GetKeyOf<IDataSource>());

        services.Configure<ExcelSyncConstraints>(config.GetSection(SyncConstraintsConfigSection));
        services.AddKeyedScoped<IAuthTester, AuthTester>(connectorType.GetKeyOf<IAuthTester>());
        
        return services;
    }
}