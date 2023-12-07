using NetworkPerspective.Sync.Infrastructure.Slack.Client.HttpClients;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Tests.Client.HttpClients
{
    public class SampleResponse : IResponseWithError
    {
        public bool IsOk { get; set; }
        public string Error { get; set; }
    }
}