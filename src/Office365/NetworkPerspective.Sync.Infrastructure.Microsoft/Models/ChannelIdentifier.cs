namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Models
{
    internal class ChannelIdentifier
    {
        public string ChannelId { get; }
        public string TeamId { get; }

        private ChannelIdentifier(string teamId, string channelId)
        {
            TeamId = teamId;
            ChannelId = channelId;
        }

        public static ChannelIdentifier Create(string teamId, string channelId)
            => new ChannelIdentifier(teamId, channelId);
    }
}