namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos
{
    internal interface ICursorPagination
    {
        public MetadataResponse Metadata { get; set; }
    }
}