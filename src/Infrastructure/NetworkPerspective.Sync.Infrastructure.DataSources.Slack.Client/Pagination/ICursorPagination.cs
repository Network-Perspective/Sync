namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.Pagination
{
    internal interface ICursorPagination
    {
        public MetadataResponse Metadata { get; set; }
    }
}