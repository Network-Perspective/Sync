﻿namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos
{
    internal class AdminConversationInvite : IResponseWithError
    {
        public bool IsOk { get; set; }
        public string Error { get; set; }
    }
}
