using System.Collections.Generic;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Pagination;

public interface IPaginatedResponse<T>
{
    int StartAt { get; set; }
    int MaxResults { get; set; }
    int Total { get; set; }
    bool IsLast { get; set; }
    IReadOnlyCollection<T> Values { get; }
}