using Microsoft.Extensions.DependencyInjection;

namespace NetworkPerspective.Sync.Utils.CQS;

public static class ServiceCollectionExctensions
{
    public static ICqsBuilder AddCqs(this IServiceCollection services)
    {
        services.AddSingleton<IMediator, Mediator>();

        return new CqsBuilder(services);
    }
}
