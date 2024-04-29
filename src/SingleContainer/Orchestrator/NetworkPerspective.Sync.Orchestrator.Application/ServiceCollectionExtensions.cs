﻿using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Orchestrator.Application.Services;

namespace NetworkPerspective.Sync.Orchestrator.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrchestratorApplication(this IServiceCollection services)
    {
        services.AddSingleton<IConnectionsLookupTable, ConnectionsLookupTable>();

        return services;
    }
}