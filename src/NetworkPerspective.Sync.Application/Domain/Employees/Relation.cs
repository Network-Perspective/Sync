namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class Relation
    {
        public const string SupervisorRelationName = "Supervisor";

        public string Name { get; init; }
        public string TargetEmployeeEmail { get; init; }
        public bool IsHashed { get; init; }

        public Relation()
        { }

        private Relation(string name, string targetEmployeeEmail, bool isHashed)
        {
            Name = name;
            TargetEmployeeEmail = targetEmployeeEmail;
            IsHashed = isHashed;
        }

        public static Relation Create(string name, string targetEmployeeEmail)
            => new Relation(name, targetEmployeeEmail, false);

        public Relation Hash(HashFunction.Delegate hash)
            => new Relation(Name, hash(TargetEmployeeEmail), true);

        public override string ToString()
            => IsHashed ? $"{Name}: {TargetEmployeeEmail}" : $"{Name}: ***";
    }
}