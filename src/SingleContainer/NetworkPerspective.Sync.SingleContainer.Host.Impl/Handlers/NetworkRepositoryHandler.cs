namespace NetworkPerspective.Sync.SingleContainer.Host.Impl.Handlers;

public class NetworkRepositoryHandler : IRpcHandler<FindNetwork, FindNetworkResult>
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
