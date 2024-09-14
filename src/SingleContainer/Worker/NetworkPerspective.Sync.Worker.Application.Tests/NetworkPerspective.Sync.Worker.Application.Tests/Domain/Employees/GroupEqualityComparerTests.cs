using System.Collections.Generic;

using FluentAssertions;

using NetworkPerspective.Sync.Worker.Application.Domain.Employees;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.Domain.Employees
{
    public class GroupEqualityComparerTests
    {
        private static readonly IEqualityComparer<Group> EqualityComparer = new GroupEqualityComparer();

        [Theory]
        [InlineData("id", "name", "Department", null, "id", "name", "Department", null)]
        [InlineData("id", "name", "Department", "parentId", "id", "name", "Department", "parentId")]
        public void ShouldIndicateEquality(string id1, string name1, string category1, string parentId1, string id2, string name2, string category2, string parentId2)
        {
            // Arrange
            var group1 = Group.CreateWithParentId(id1, name1, category1, parentId1);
            var group2 = Group.CreateWithParentId(id2, name2, category2, parentId2);

            // Act
            var result = EqualityComparer.Equals(group1, group2);

            // Assert
            result.Should().BeTrue();
        }

        [Theory]
        [InlineData("id", "name", "Department", null, "id_other", "name", "Department", null)]
        [InlineData("id", "name", "Department", null, "id", "name_other", "Department", null)]
        [InlineData("id", "name", "Department", null, "id", "name", "Project", null)]
        [InlineData("id", "name", "Department", null, "id", "name", "Department", "parentId")]
        public void ShouldIndicateInequality(string id1, string name1, string category1, string parentId1, string id2, string name2, string category2, string parentId2)
        {
            // Arrange
            var group1 = Group.CreateWithParentId(id1, name1, category1, parentId1);
            var group2 = Group.CreateWithParentId(id2, name2, category2, parentId2);

            // Act
            var result = EqualityComparer.Equals(group1, group2);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("id", "name", "Department", null)]
        [InlineData("id", "name", "Department", "parentId")]
        public void ShouldBeAbleToCalculateHashcode(string id, string name, string category, string parentId)
        {
            // Arrange
            var group = Group.CreateWithParentId(id, name, category, parentId);

            // Act Assert
            EqualityComparer.GetHashCode(group);
        }
    }
}