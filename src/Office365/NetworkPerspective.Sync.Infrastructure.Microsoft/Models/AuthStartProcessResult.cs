namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Models
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