namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos
{
    public interface IResponseWithError
    {
        public bool IsOk { get; set; }

        public string Error { get; set; }
    }
}