using System;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Models
{
    internal class ChatMessage
    {
        public Chat Chat { get; set; }
        public string Id { get; }
        public string SenderId { get; set; }
        public DateTime TimeStamp { get; set; }

        public ChatMessage(string id, string senderId, DateTime timeStamp, Chat chat)
        {
            Id = id;
            SenderId = senderId;
            TimeStamp = timeStamp;
            Chat = chat;
        }
    }
}
