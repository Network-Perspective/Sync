using System;

namespace NetworkPerspective.Sync.Application.Domain.Statuses
{
    public class StatusLog
    {
        public Guid NetworkId { get; }
        public DateTime TimeStamp { get; }
        public string Message { get; }
        public StatusLogLevel Level { get; }

        private StatusLog(Guid networkId, string message, StatusLogLevel level, DateTime timestamp)
        {
            NetworkId = networkId;
            Message = message;
            Level = level;
            TimeStamp = timestamp;
        }

        public static StatusLog Create(Guid networkId, string message, StatusLogLevel level, DateTime timestamp)
            => new StatusLog(networkId, message, level, timestamp);
    }
}