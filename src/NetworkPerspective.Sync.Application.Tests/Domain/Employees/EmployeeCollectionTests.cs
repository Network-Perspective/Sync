using System;
using System.Linq;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Employees;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Employees
{
    public class EmployeeCollectionTests
    {
        [Fact]
        public void ShouldResolveEmailAsMainAddress()
        {
            // Arrange
            const string mainEmail = "email_1@networkparspective.io";

            const string id = "HGFD";

            const string alias_1 = "email_1_alias_1@networkperspective.io";
            const string alias_2 = "U12345ASD";

            var employeeId = EmployeeId.CreateWithAliases(mainEmail, id, new[] { alias_1, alias_2 });
            var employee = Employee.CreateInternal(employeeId, Array.Empty<Group>());
            var employeesCollection = new EmployeeCollection(new[] { employee }, null);

            // Act
            var employee1 = employeesCollection.Find(mainEmail);
            var employee2 = employeesCollection.Find(alias_1);
            var employee3 = employeesCollection.Find(alias_2);
            var employee4 = employeesCollection.Find(id);

            // Assert
            employee1.Id.PrimaryId.Should().Be(mainEmail);
            employee2.Id.PrimaryId.Should().Be(mainEmail);
            employee3.Id.PrimaryId.Should().Be(mainEmail);
            employee4.Id.PrimaryId.Should().Be(mainEmail);
            employee1.IsExternal.Should().BeFalse();
            employee2.IsExternal.Should().BeFalse();
            employee3.IsExternal.Should().BeFalse();
            employee4.IsExternal.Should().BeFalse();
        }

        [Fact]
        public void ShouldResolveUnkwnownAliasAsExternal()
        {
            // Arrange
            const string unkwnownAlias = "foo";
            var employeesCollection = new EmployeeCollection(Enumerable.Empty<Employee>(), null);

            // Act
            var email = employeesCollection.Find(unkwnownAlias);

            // Assert
            email.IsExternal.Should().BeTrue();
            email.Id.PrimaryId.Should().Be(unkwnownAlias);
        }

        [Fact]
        public void ShouldCorrectlyEvaluteHierarchy()
        {
            // Arrange
            var ic1 = Employee.CreateInternal(EmployeeId.Create("ic1", string.Empty), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "manager1") }));
            var ic2 = Employee.CreateInternal(EmployeeId.Create("ic2", string.Empty), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "manager1_1") }));
            var ic3 = Employee.CreateInternal(EmployeeId.Create("ic3", string.Empty), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "manager2") }));
            var ic4 = Employee.CreateInternal(EmployeeId.Create("ic4", string.Empty), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "manager3") }));
            var ic5 = Employee.CreateInternal(EmployeeId.Create("ic5", string.Empty), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "director1") }));

            var manager1 = Employee.CreateInternal(EmployeeId.CreateWithAliases("manager1", string.Empty, new[] { "manager1_1" }), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "director1") }));
            var manager2 = Employee.CreateInternal(EmployeeId.Create("manager2", string.Empty), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "director2") }));
            var manager3 = Employee.CreateInternal(EmployeeId.Create("manager3", string.Empty), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "board2") }));

            var director1 = Employee.CreateInternal(EmployeeId.Create("director1", string.Empty), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "board1") }));
            var director2 = Employee.CreateInternal(EmployeeId.Create("director2", string.Empty), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "board1") }));

            var board1 = Employee.CreateInternal(EmployeeId.Create("board1", string.Empty), Array.Empty<Group>(), null, RelationsCollection.Empty);
            var board2 = Employee.CreateInternal(EmployeeId.Create("board2", string.Empty), Array.Empty<Group>(), null, RelationsCollection.Empty);

            // Act
            var employees = new[] { ic1, ic2, ic3, ic4, ic5, manager1, manager2, manager3, director1, director2, board1, board2 };

            var employeesCollection = new EmployeeCollection(employees, null);


            // Assert
            ic1.Props["Hierarchy"].Should().Be(EmployeeHierarchy.IndividualContributor);
            ic2.Props["Hierarchy"].Should().Be(EmployeeHierarchy.IndividualContributor);
            ic3.Props["Hierarchy"].Should().Be(EmployeeHierarchy.IndividualContributor);
            ic4.Props["Hierarchy"].Should().Be(EmployeeHierarchy.IndividualContributor);
            ic5.Props["Hierarchy"].Should().Be(EmployeeHierarchy.IndividualContributor);

            manager1.Props["Hierarchy"].Should().Be(EmployeeHierarchy.Manager);
            manager2.Props["Hierarchy"].Should().Be(EmployeeHierarchy.Manager);
            manager3.Props["Hierarchy"].Should().Be(EmployeeHierarchy.Manager);

            director1.Props["Hierarchy"].Should().Be(EmployeeHierarchy.Director);
            director2.Props["Hierarchy"].Should().Be(EmployeeHierarchy.Director);

            board1.Props["Hierarchy"].Should().Be(EmployeeHierarchy.Board);
            board2.Props["Hierarchy"].Should().Be(EmployeeHierarchy.Board);
        }
    }
}