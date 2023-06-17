namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Pagination
{
    internal interface ICursorPagination
    {
        public MetadataResponse Metadata { get; set; }
    }
}