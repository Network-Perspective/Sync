using System;

namespace NetworkPerspective.Sync.Worker.Application.Extensions;

public static class DateTimeExtensions
{
    public static DateTime Bucket(this DateTime datetime, TimeSpan roundingInterval)
    {
        var divisionResult = Convert.ToInt64(datetime.Ticks / (decimal)roundingInterval.Ticks);
        return new DateTime(divisionResult * roundingInterval.Ticks);
    }
}