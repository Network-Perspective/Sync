using System;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Model;

internal class ProjectContainer
{
    public Guid Id { get; }

    public ProjectContainer(Guid id)
    {
        Id = id;
    }
}