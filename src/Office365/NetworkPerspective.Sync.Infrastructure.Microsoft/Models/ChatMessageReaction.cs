using System;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Models
{
    internal class ChatMessageReaction
    {
        public string Id { get; }
        public string SenderId { get; }
        public DateTime TimeStamp { get; }
        public ChatMessage ChatMessage { get; }

        public ChatMessageReaction(string id, string senderId, DateTime timeStamp, ChatMessage chatMessage)
        {
            Id = id;
            SenderId = senderId;
            TimeStamp = timeStamp;
            ChatMessage = chatMessage;
        }
    }
}
