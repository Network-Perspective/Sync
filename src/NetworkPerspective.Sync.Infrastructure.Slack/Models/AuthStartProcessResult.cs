namespace NetworkPerspective.Sync.Infrastructure.Slack.Models
{
    public class AuthStartProcessResult
    {
        public string SlackAuthUri { get; }

        public AuthStartProcessResult(string slackAuthUri)
        {
            SlackAuthUri = slackAuthUri;
        }
    }
}