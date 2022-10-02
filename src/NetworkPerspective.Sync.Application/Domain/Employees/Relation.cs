using System;

namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class Relation
    {
        public string Name { get; }
        public string TargetEmployeeEmail { get; }
        public bool IsHashed { get; }

        private Relation(string name, string targetEmployeeEmail, bool isHashed)
        {
            Name = name;
            TargetEmployeeEmail = targetEmployeeEmail;
            IsHashed = isHashed;
        }

        public static Relation Create(string name, string targetEmployeeEmail)
            => new Relation(name, targetEmployeeEmail, false);

        public Relation Hash(Func<string, string> hash)
            => new Relation(Name, hash(TargetEmployeeEmail), true);

        public override string ToString()
            => IsHashed ? $"{Name}: {TargetEmployeeEmail}" : $"{Name}: ***";
    }
}