﻿using System;
using System.Diagnostics.CodeAnalysis;

namespace NetworkPerspective.Sync.Utils.Models;

public sealed class TimeRange : IEquatable<TimeRange>
{
    public static TimeRange Empty => new(null, null);

    public const string DefaultDateTimeFormat = "yyyy-MM-dd HH:mm";

    public DateTime Start { get; }
    public DateTime End { get; }
    public TimeSpan Duration => End - Start;


    public TimeRange(DateTime? start, DateTime? end)
    {
        Start = start ?? DateTime.MinValue;
        End = end ?? DateTime.MaxValue;
    }

    public bool IsInRange(DateTime? dt)
    {
        if (dt == null)
            return false;

        var downLimitSatisfied = Start < dt;
        var upLimitSatisfied = dt < End;

        return downLimitSatisfied && upLimitSatisfied;
    }

    public override bool Equals(object obj)
        => Equals(obj as TimeRange);

    public bool Equals([AllowNull] TimeRange other)
    {
        if (other == null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return Start == other.Start && End == other.End;
    }

    public override int GetHashCode()
        => HashCode.Combine(Start, End);

    public override string ToString()
        => $"{Start.ToString(DefaultDateTimeFormat)} - {End.ToString(DefaultDateTimeFormat)}";
}