namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models
{
    internal class Team
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Team(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}