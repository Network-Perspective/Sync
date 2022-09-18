namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos
{
    internal interface IResponseWithError
    {
        public bool IsOk { get; set; }

        public string Error { get; set; }
    }
}