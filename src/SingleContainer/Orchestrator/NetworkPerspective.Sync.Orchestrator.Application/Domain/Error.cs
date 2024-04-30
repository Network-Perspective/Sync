namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class Error
{
    public static class Types
    {
        public const string Unknown = "Unknown";
        public const string Security = "Security";
        public const string Client = "Client";
        public const string NetworkPerspectiveCore = "Infrastructure - Network Perspective Core";
        public const string SecretStorage = "Infrastructure - Secret Storage";
        public const string Database = "Infrastructure - Database";
        public const string Application = "Application";
    }

    public string Type { get; }
    public string Title { get; }
    public string Details { get; }
    public int StatusCode { get; }

    public Error(string type, string title, string details, int statusCode)
    {
        Type = type;
        Title = title;
        Details = details;
        StatusCode = statusCode;
    }
}
