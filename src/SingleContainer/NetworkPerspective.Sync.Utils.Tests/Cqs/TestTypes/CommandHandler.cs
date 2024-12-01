using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Utils.CQS.Commands;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

internal class CommandHandler : ICommandHandler<CommandRequest>
{
    public Task HandleAsync(CommandRequest request, CancellationToken stoppingToken = default)
        => Task.CompletedTask;
}
