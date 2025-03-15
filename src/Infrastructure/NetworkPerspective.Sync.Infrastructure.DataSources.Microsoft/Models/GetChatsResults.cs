using System;
using System.Collections.Generic;

using InternalChat = NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models.Chat;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models;

internal class GetChatsResults
{
    public IEnumerable<InternalChat> Chats { get; }
    public Exception Exception { get; }

    private GetChatsResults(IEnumerable<InternalChat> chats, Exception exception)
    {
        Chats = chats;
        Exception = exception;
    }

    public static GetChatsResults Error(Exception exception)
        => new([], exception);

    public static GetChatsResults Success(IEnumerable<InternalChat> chats)
        => new(chats, null);
}