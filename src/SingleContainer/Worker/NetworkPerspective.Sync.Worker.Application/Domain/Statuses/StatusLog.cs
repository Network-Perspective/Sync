using System;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Statuses
{
    public class StatusLog
    {
        public Guid ConnectorId { get; }
        public DateTime TimeStamp { get; }
        public string Message { get; }
        public StatusLogLevel Level { get; }

        private StatusLog(Guid connectorId, string message, StatusLogLevel level, DateTime timestamp)
        {
            ConnectorId = connectorId;
            Message = message;
            Level = level;
            TimeStamp = timestamp;
        }

        public static StatusLog Create(Guid connectorId, string message, StatusLogLevel level, DateTime timestamp)
            => new StatusLog(connectorId, message, level, timestamp);
    }
}