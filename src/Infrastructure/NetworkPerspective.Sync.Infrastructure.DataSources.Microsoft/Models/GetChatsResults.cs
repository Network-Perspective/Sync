using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models;

internal class GetChatsResults
{
    public IEnumerable<Chat> Chats { get; }
    public Exception Exception { get; }

    private GetChatsResults(IEnumerable<Chat> chats, Exception exception)
    {
        Chats = chats;
        Exception = exception;
    }

    public static GetChatsResults Error(Exception exception)
        => new([], exception);

    public static GetChatsResults Success(IEnumerable<Chat> chats)
        => new(chats, null);
}