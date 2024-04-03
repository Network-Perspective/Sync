using System;
using System.Text.Json.Serialization;

namespace NetworkPerspective.Sync.Application.Domain.Statuses
{
    public class StatusLog
    {
        public Guid NetworkId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Message { get; set; }
        public StatusLogLevel Level { get; set; }
        
        [JsonConstructor]
        public StatusLog() {}

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