namespace NetworkPerspective.Sync.Orchestrator.Application.Domain.Statuses;

public class ConnectorTaskStatus
{
    public static readonly ConnectorTaskStatus Empty = new(string.Empty, string.Empty, null);

    public string Caption { get; set; }
    public string Description { get; set; }
    public double? CompletionRate { get; set; }

    private ConnectorTaskStatus(string caption, string description, double? completionRate)
    {
        Caption = caption;
        Description = description;
        CompletionRate = completionRate;
    }

    public static ConnectorTaskStatus Create(string caption, string description, double? completionRate)
        => new(caption, description, completionRate);

    public override string ToString()
    {
        var completionRate = CompletionRate is null ? "???" : $"{CompletionRate: 0.##}%";
        return $"{Caption}: {completionRate} ({Description})";
    }
}