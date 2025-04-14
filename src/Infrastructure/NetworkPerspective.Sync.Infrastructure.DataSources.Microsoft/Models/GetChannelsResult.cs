using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models;

internal class GetChannelsResult
{
    public IEnumerable<Channel> Channels { get; }
    public Exception Exception { get; }

    private GetChannelsResult(IEnumerable<Channel> channels, Exception exception)
    {
        Channels = channels;
        Exception = exception;
    }

    public static GetChannelsResult Error(Exception exception)
        => new([], exception);

    public static GetChannelsResult Success(IEnumerable<Channel> channels)
        => new(channels, null);
}