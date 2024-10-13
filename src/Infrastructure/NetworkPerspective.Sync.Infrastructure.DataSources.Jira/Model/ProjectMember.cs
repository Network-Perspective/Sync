using System.Collections.Generic;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Model;

internal class ProjectMember
{
    public string Id { get; }
    public string Mail { get; }
    public List<Project> Projects { get; } = [];

    public ProjectMember(string id, string mail)
    {
        Id = id;
        Mail = mail;
    }
}