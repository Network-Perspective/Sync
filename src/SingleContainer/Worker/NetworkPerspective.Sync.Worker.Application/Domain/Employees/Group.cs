using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Worker.Application.Domain;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Employees
{
    public class Group
    {
        public const string CompanyCatergory = "OrgUnitCompany";
        public const string TeamCatergory = "OrgUnitTeam";
        public const string DepartmentCatergory = "Department";
        public const string ChannelCategory = "Channel";

        public static IEqualityComparer<Group> EqualityComparer = new GroupEqualityComparer();

        public string Id { get; }
        public string Name { get; }
        public string Category { get; }
        public string ParentId { get; }
        public bool IsHashed { get; }

        private Group(string id, string name, string category, string parentId, bool isHashed)
        {
            Id = id;
            Name = name;
            Category = category;
            ParentId = parentId;
            IsHashed = isHashed;
        }

        public static Group Create(string id, string name, string category)
            => new Group(id, name, category, null, false);

        public static Group CreateWithParentId(string id, string name, string category, string parentId)
            => new Group(id, name, category, parentId, false);

        public Group Hash(HashFunction.Delegate hashFunc)
        {
            if (IsHashed)
                throw new InvalidOperationException("Group is already hashed. Hashing twice is just silly... isn't it?");

            var hashedId = hashFunc(Id);
            var hashedParentId = ParentId == null ? null : hashFunc(ParentId);

            return new Group(hashedId, Name, Category, hashedParentId, true);
        }
    }
}