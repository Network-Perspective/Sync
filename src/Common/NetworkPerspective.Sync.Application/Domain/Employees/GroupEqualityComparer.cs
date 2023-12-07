using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class GroupEqualityComparer : IEqualityComparer<Group>
    {
        private static readonly IEqualityComparer<string> StringEqualityComparer = StringComparer.InvariantCultureIgnoreCase;

        public bool Equals(Group x, Group y)
        {
            if (x == null || y == null)
                return false;

            if (ReferenceEquals(x, y))
                return true;

            if (x.GetType() != y.GetType())
                return false;

            var idEquals = StringEqualityComparer.Equals(x.Id, y.Id);
            var nameEquals = StringEqualityComparer.Equals(x.Name, y.Name);
            var categoryEquals = StringEqualityComparer.Equals(x.Category, y.Category);
            var parentIdEquals = IsParentIdEqual(x.ParentId, y.ParentId);

            return idEquals && nameEquals && categoryEquals && parentIdEquals;
        }

        private static bool IsParentIdEqual(string xParentId, string yParentId)
        {
            if (xParentId == null && yParentId == null)
                return true;

            if (xParentId == null || yParentId == null)
                return false;

            return StringEqualityComparer.Equals(xParentId, yParentId);
        }

        public int GetHashCode([DisallowNull] Group obj)
            => HashCode.Combine(obj.Id, obj.Name, obj.Category, obj.ParentId);
    }
}