using System;
using System.Diagnostics.CodeAnalysis;

namespace NetworkPerspective.Sync.Application.Domain
{
    public sealed class TimeRange : IEquatable<TimeRange>
    {
        public DateTime Start { get; }
        public DateTime End { get; }

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
            => $"{Start} - {End}";
    }
}