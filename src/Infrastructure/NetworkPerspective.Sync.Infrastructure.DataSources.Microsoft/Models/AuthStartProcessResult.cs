namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models
{
    public class AuthStartProcessResult
    {
        public string MicrosoftAuthUri { get; }

        public AuthStartProcessResult(string microsoftAuthUri)
        {
            MicrosoftAuthUri = microsoftAuthUri;
        }
    }
}