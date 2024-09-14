using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Worker.Application.Domain.Interactions;

public interface IInteractionsStream : IAsyncDisposable
{
    Task<int> SendAsync(IEnumerable<Interaction> interactions);
}