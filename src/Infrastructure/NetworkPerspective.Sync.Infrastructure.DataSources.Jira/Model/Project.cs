namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Model;

internal class Project
{
    public string Id { get; }
    public string Name { get; }
    public ProjectContainer Container { get; }

    public Project(string id, string name, ProjectContainer container)
    {
        Id = id;
        Name = name;
        Container = container;
    }
}