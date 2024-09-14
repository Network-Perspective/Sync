using FluentAssertions;

using NetworkPerspective.Sync.Worker.Application.Domain.Employees;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.Domain.Employees
{
    public class RelationTests
    {
        [Fact]
        public void ShouldCreateNotHashedRelation()
        {
            // Arrange
            const string targetEmployeeEmail = "alice@networkperspective.io";
            const string relationName = "Supervisor";

            // Act
            var relation = Relation.Create(relationName, targetEmployeeEmail);

            // Assert
            relation.IsHashed.Should().BeFalse();
            relation.Name.Should().Be(relationName);
            relation.TargetEmployeeEmail.Should().Be(targetEmployeeEmail);
        }

        [Fact]
        public void ShouldHash()
        {
            // Arrange
            const string targetEmployeeEmail = "alice@networkperspective.io";
            const string relationName = "Supervisor";

            var relation = Relation.Create(relationName, targetEmployeeEmail);

            // Act
            var hashedRelation = relation.Hash(x => $"asd_hashed");

            // Assert
            hashedRelation.IsHashed.Should().BeTrue();
            hashedRelation.TargetEmployeeEmail.Should().Be("asd_hashed");
            hashedRelation.ToString().Should().NotContain(targetEmployeeEmail);
        }
    }
}