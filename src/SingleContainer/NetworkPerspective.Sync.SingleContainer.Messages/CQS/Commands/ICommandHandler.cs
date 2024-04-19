using System.Threading.Tasks;


namespace NetworkPerspective.Sync.SingleContainer.Messages.CQS.Commands;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task HandleAsync(TCommand command);
}