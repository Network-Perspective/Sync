using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain.Interactions;

public interface IInteractionsStream : IAsyncDisposable
{
    Task SendAsync(IEnumerable<Interaction> interactions);
}