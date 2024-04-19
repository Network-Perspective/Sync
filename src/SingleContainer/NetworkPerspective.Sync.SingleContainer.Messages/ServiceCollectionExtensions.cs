using System;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.SingleContainer.Messages.CQS;
using NetworkPerspective.Sync.SingleContainer.Messages.CQS.Commands;
using NetworkPerspective.Sync.SingleContainer.Messages.CQS.Queries;

namespace NetworkPerspective.Sync.SingleContainer.Messages;

public static class MessageHandlerExtensions
{
    public static void RegisterMessageHandlers(this IServiceCollection services, params Assembly[] assemblies)
    {
        services.AddTransient<IMessageSerializer, MessageSerializer>();

        services.AddTransient<IQueryDispatcher, QueryDispatcher>();
        services.AddTransient<ICommandDispatcher, CommandDispatcher>();

        foreach (var assembly in assemblies)
        {
            services.RegisterCommandHandlersFromAssembly(assembly);
            services.RegisterQueryHandlersFromAssembly(assembly);
        }
    }

    private static void RegisterCommandHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
        => services.RegisterAllImplementationsFromAssembly(assembly, typeof(ICommandHandler<>));

    private static void RegisterQueryHandlersFromAssembly(this IServiceCollection services, Assembly assembly)
        => services.RegisterAllImplementationsFromAssembly(assembly, typeof(IQueryHandler<,>));

    private static void RegisterAllImplementationsFromAssembly(this IServiceCollection services, Assembly assembly, Type type)
    {
        assembly
            .ExportedTypes
            .Where(t => t.IsClass)
            .SelectMany(t => t.GetInterfaces(), (c, i) => new { Class = c, Interface = i })
            .Where(x => x.Interface.IsGenericType &&
                        x.Interface.GetGenericTypeDefinition() == type)
            .ToList()
            .ForEach(x => services.AddTransient(x.Interface, x.Class));
    }
}