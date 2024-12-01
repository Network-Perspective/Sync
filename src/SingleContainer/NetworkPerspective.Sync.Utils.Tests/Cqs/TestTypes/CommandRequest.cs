using System;

using NetworkPerspective.Sync.Utils.CQS.Commands;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

internal class CommandRequest : ICommand 
{
    public Guid CorrelationId { get; set; }
}
