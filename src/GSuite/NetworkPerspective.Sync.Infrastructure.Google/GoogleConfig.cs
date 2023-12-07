namespace NetworkPerspective.Sync.Infrastructure.Google
{
    public class GoogleConfig
    {
        public string ApplicationName { get; set; }
        public int SyncOverlapInMinutes { get; set; }
        public int MaxMessagesPerUserDaily { get; set; }
    }
}