namespace NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Client.HttpClients
{
    public interface IResponseWithError
    {
        public bool IsOk { get; set; }

        public string Error { get; set; }
    }
}