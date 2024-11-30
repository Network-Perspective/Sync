using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.CQS;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

internal class CommandHandler : IRequestHandler<CommandRequest>
{
    public Task HandleAsync(CommandRequest request, CancellationToken stoppingToken = default)
        => Task.CompletedTask;
}
