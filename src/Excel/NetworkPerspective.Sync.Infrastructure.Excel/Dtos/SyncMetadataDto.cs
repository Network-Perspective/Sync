namespace NetworkPerspective.Sync.Infrastructure.Excel.Dtos;

public class SyncMetadataDto
{
    public SyncMetadataIncludesDto Plain { get; set; }
    public SyncMetadataIncludesDto Hashed { get; set; }
}

public class SyncMetadataIncludesDto
{
    public HashSet<string> Props { get; set; }
    public HashSet<string> Relationships { get; set; }
    public HashSet<string> Groups { get; set; }
}