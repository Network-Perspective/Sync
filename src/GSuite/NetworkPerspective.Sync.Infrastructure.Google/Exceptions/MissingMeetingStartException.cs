using System;

namespace NetworkPerspective.Sync.Infrastructure.Google.Exceptions
{
    internal class MissingMeetingStartException : Exception
    {
        public MissingMeetingStartException() : base("Meeting has no start timestamp")
        { }
    }
}