namespace NetworkPerspective.Sync.Worker.Application.Domain.Statuses;

public class SingleTaskStatus
{
    public static readonly SingleTaskStatus Empty = new(string.Empty, string.Empty, null);

    public string Caption { get; set; }
    public string Description { get; set; }
    public double? CompletionRate { get; set; }

    private SingleTaskStatus(string caption, string description, double? completionRate)
    {
        Caption = caption;
        Description = description;
        CompletionRate = completionRate;
    }

    public static SingleTaskStatus New(string caption, string description, double? completionRate)
        => new(caption, description, completionRate);

    public static SingleTaskStatus WithUnknownProgress(string caption, string description)
        => new(caption, description, null);

    public override string ToString()
    {
        var completionRate = CompletionRate is null ? "???" : $"{CompletionRate: 0.##}%";
        return $"{Caption}: {completionRate} ({Description})";
    }
}