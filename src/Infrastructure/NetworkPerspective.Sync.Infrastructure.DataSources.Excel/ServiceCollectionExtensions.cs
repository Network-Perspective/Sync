using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Excel;

public static class ServiceCollectionExtensions
{
    private const string SyncConstraintsConfigSection = "SyncConstraints";
    public static IServiceCollection AddExcel(this IServiceCollection services, IConfigurationSection config, ConnectorType connectorType)
    {
        services.AddTransient<ICapabilityTester>(x =>
        {
            var logger = x.GetRequiredService<ILogger<CapabilityTester>>();
            return new CapabilityTester(connectorType, logger);
        });

        services.AddKeyedScoped<IDataSource, ExcelFacade>(connectorType.GetKeyOf<IDataSource>());

        services.Configure<ExcelSyncConstraints>(config.GetSection(SyncConstraintsConfigSection));
        services.AddKeyedScoped<IAuthTester, AuthTester>(connectorType.GetKeyOf<IAuthTester>());

        return services;
    }
}