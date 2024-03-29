using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace NetworkPerspective.Sync.SingleContainer.Messages.Services;

public static class MessageHandlerExtensions
{
    public static void RegisterMessageHandlers(this IServiceCollection services, Assembly assembly)
    {
        services.AddTransient<IMessageSerializer, MessageSerializer>();
        services.AddTransient<IMessageDispatcher, MessageDispatcher>();

        assembly
            .ExportedTypes
            .Where(t => t.IsClass)
            .SelectMany(t => t.GetInterfaces(), (c, i) => new { Class = c, Interface = i })
            .Where(x => x.Interface.IsGenericType &&
                        x.Interface.GetGenericTypeDefinition() == typeof(IMessageHandler<>))
            .ToList()
            .ForEach(x => services.AddTransient(x.Interface, x.Class));
    }
}