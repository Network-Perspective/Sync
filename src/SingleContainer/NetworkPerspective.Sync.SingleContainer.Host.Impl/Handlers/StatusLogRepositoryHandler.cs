namespace NetworkPerspective.Sync.SingleContainer.Host.Impl.Handlers;

public class StatusLogRepositoryHandler(ILogger<StatusLogRepositoryHandler> logger) : IRpcHandler<StatusLogAdd, StatusLogAddResult>
{
    public async Task<StatusLogAddResult> HandleAsync(StatusLogAdd args)
    {
        logger.LogDebug("Received log message: {msg}", args.Log.Message);
        // todo save to db
        return await Task.FromResult(new StatusLogAddResult());
    }
}