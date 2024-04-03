using NetworkPerspective.Sync.Application.Domain.Statuses;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Repositories;
using NetworkPerspective.Sync.SingleContainer.Connector.Transport;
using NetworkPerspective.Sync.SingleContainer.Messages.Services;

namespace NetworkPerspective.Sync.SingleContainer.Messages;

public class RemoteStatusLogRepository(IHostConnection conn) : IStatusLogRepository
{
    public async Task<IEnumerable<StatusLog>> GetListAsync(Guid networkId, CancellationToken stoppingToken = default)
    {
        return (await conn.CallAsync<StatusLogGetListResult>(new Messages.StatusLogGetList(networkId))).Logs;
    }

    public async Task AddAsync(StatusLog log, CancellationToken stoppingToken = default)
    {
        await conn.CallAsync<StatusLogAddResult>(new StatusLogAdd(log));
    }
}

public record StatusLogGetList(Guid NetworkId) : IRpcArgs;
public record StatusLogGetListResult(List<StatusLog> Logs) : IRpcResult;

public record StatusLogAdd(StatusLog Log) : IRpcArgs;
public record StatusLogAddResult() : IRpcResult;