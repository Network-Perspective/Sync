using System;

namespace NetworkPerspective.Sync.Utils.CQS.Commands;

public interface ICommand
{
    Guid CorrelationId { get; set; }
}
