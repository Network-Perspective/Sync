using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Contract;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Stub;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreStub(this IServiceCollection services)
    {
        services.AddTransient<ICore, CoreStub>();

        return services;
    }
}
