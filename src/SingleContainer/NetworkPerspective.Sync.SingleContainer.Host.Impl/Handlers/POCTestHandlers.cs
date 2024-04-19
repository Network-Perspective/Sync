using NetworkPerspective.Sync.SingleContainer.Host.Impl.Transport;
using NetworkPerspective.Sync.SingleContainer.Messages.CQS.Commands;
using NetworkPerspective.Sync.SingleContainer.Messages.CQS.Queries;

namespace NetworkPerspective.Sync.SingleContainer.Host.Impl.Handlers;

public class NetworkRepositoryHandler : IQueryHandler<FindNetwork, FindNetworkResult>
{
    public Task<FindNetworkResult> HandleAsync(FindNetwork args)
    {
        var props = new Dictionary<string, string>();
        props["networkId"] = args.NetworkId.ToString();
        props["networkName"] = "TestNetwork";
        props["now"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        return Task.FromResult(new FindNetworkResult(props));
    }
}

public class PingHandler(ILogger<PingHandler> logger, IConnectorContext context) : ICommandHandler<Ping>
{
    public Task HandleAsync(Ping msg)
    {
        logger.LogInformation("Received ping message: {message} from {connectorName}",
            msg.Message, context.Name);
        return Task.CompletedTask;
    }
}