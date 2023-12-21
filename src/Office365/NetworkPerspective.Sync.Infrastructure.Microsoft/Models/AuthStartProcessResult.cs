namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Models
{
    internal class AuthStartProcessResult
    {
        public string MicrosoftAuthUri { get; }

        public AuthStartProcessResult(string microsoftAuthUri)
        {
            MicrosoftAuthUri = microsoftAuthUri;
        }
    }
}