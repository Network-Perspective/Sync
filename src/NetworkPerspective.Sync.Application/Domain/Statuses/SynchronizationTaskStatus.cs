namespace NetworkPerspective.Sync.Application.Domain.Statuses
{
    public class SynchronizationTaskStatus
    {
        public static readonly SynchronizationTaskStatus Empty = new SynchronizationTaskStatus(string.Empty, string.Empty, 0);

        public string Caption { get; set; }
        public string Description { get; set; }
        public double CompletionRate { get; set; }

        public SynchronizationTaskStatus(string caption, string description, double completionRate)
        {
            Caption = caption;
            Description = description;
            CompletionRate = completionRate;
        }
    }
}