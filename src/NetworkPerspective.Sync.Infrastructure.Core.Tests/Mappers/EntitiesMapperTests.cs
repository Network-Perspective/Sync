using System;
using System.Collections.Generic;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Infrastructure.Core.Mappers;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Core.Tests.Mappers
{
    public class EntitiesMapperTests
    {
        [Fact]
        public void ShouldExcludeSensitiveProps()
        {
            // Arrange
            var props = new Dictionary<string, object>
            {
                {Employee.PropKeyName, "Alice Smith" },
                {Employee.PropKeyTeam, "Super Team" },
                {Employee.PropKeyDepartment, new[] {"IT", "DevOps"} }
            };
            var employee = Employee.CreateInternal(EmployeeId.Create("foo", "bar"), Array.Empty<Group>(), props);

            // Act
            var result = EntitiesMapper.ToEntity(employee, null, "test");

            // Assert
            result.Props.Should().NotContainKey(Employee.PropKeyName);
            result.Props.Should().NotContainKey(Employee.PropKeyTeam);
            result.Props.Should().NotContainKey(Employee.PropKeyDepartment);
        }

        [Fact]
        public void ShouldMapRelations()
        {
            // Arrange
            const string dataSourceId = "GSuite";
            const string managerEmail = "baz@networkperspective.io";
            const string managerDataSourceId = "someId";

            var manager = Employee.CreateInternal(EmployeeId.Create(managerEmail, managerDataSourceId), Array.Empty<Group>());

            var relations = new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, managerEmail) });

            var employee = Employee.CreateInternal(EmployeeId.Create("foo", "bar"), Array.Empty<Group>(), null, relations);

            // Act
            var result = EntitiesMapper.ToEntity(employee, manager, dataSourceId);

            // Assert
            var managerId = new Dictionary<string, string>
            {
                { "Email", managerEmail },
                { dataSourceId, managerDataSourceId }
            };
            var expectedRelations = new[] { new HashedEntityRelationship { RelationshipName = Employee.SupervisorRelationName, TargetIds = managerId } };
            result.Relationships.Should().BeEquivalentTo(expectedRelations);
        }
    }
}