using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.SingleContainer.Messages;
using NetworkPerspective.Sync.SingleContainer.Messages.CQS.Queries;

namespace NetworkPerspective.Sync.SingleContainer.Host.Impl.Handlers;

public class StatusLogRepositoryHandler(ILogger<StatusLogRepositoryHandler> logger) : IQueryHandler<StatusLogAdd, StatusLogAddResult>
{
    public async Task<StatusLogAddResult> HandleAsync(StatusLogAdd args)
    {
        logger.LogDebug("Received log message: {msg}", args.Log.Message);
        // todo save to db
        return await Task.FromResult(new StatusLogAddResult());
    }
}