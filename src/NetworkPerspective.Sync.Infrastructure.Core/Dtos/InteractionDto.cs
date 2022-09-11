using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Infrastructure.Core.Dtos
{
    internal class InteractionDto
    {
        public DateTime Timestamp { get; set; }
        public string SourceId { get; set; }
        public string TargetId { get; set; }
        public string Type { get; set; }
        public string ChannelId { get; set; }
        public string EventId { get; set; }
        public string Recurring { get; set; }
        public ISet<UserActionTypeDto> UserAction { get; set; }
        public int? Duration { get; set; }
    }
}