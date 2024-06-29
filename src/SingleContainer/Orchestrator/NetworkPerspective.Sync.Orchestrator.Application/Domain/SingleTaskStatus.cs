namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class SingleTaskStatus
{
    public static readonly SingleTaskStatus Empty = new(string.Empty, string.Empty, null);

    public string Caption { get; set; }
    public string Description { get; set; }
    public double? CompletionRate { get; set; }

    public SingleTaskStatus(string caption, string description, double? completionRate)
    {
        Caption = caption;
        Description = description;
        CompletionRate = completionRate;
    }

    public override string ToString()
    {
        var completionRate = CompletionRate is null ? "???" : $"{CompletionRate: 0.##}%";
        return $"{Caption}: {completionRate} ({Description})";
    }
}