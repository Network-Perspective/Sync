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

            var lookupTable = new EmployeeCollection(null);
            var employeeId = EmployeeId.Create(mainEmail, id);
            lookupTable.Add(Employee.CreateInternal(employeeId, Array.Empty<Group>()), new[] { alias_1, alias_2 }.ToHashSet());

            // Act
            var employee1 = lookupTable.Find(mainEmail);
            var employee2 = lookupTable.Find(alias_1);
            var employee3 = lookupTable.Find(alias_2);
            var employee4 = lookupTable.Find(id);

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
            var lookupTable = new EmployeeCollection(null);

            // Act
            var email = lookupTable.Find(unkwnownAlias);

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

            var manager1 = Employee.CreateInternal(EmployeeId.Create("manager1", string.Empty), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "director1") }));
            var manager2 = Employee.CreateInternal(EmployeeId.Create("manager2", string.Empty), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "director2") }));
            var manager3 = Employee.CreateInternal(EmployeeId.Create("manager3", string.Empty), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "board2") }));

            var director1 = Employee.CreateInternal(EmployeeId.Create("director1", string.Empty), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "board1") }));
            var director2 = Employee.CreateInternal(EmployeeId.Create("director2", string.Empty), Array.Empty<Group>(), null, new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, "board1") }));

            var board1 = Employee.CreateInternal(EmployeeId.Create("board1", string.Empty), Array.Empty<Group>(), null, RelationsCollection.Empty);
            var board2 = Employee.CreateInternal(EmployeeId.Create("board2", string.Empty), Array.Empty<Group>(), null, RelationsCollection.Empty);

            var employeesCollection = new EmployeeCollection(null);

            // Act
            employeesCollection.Add(ic1, Array.Empty<string>().ToHashSet());
            employeesCollection.Add(ic2, Array.Empty<string>().ToHashSet());
            employeesCollection.Add(ic3, Array.Empty<string>().ToHashSet());
            employeesCollection.Add(ic4, Array.Empty<string>().ToHashSet());
            employeesCollection.Add(ic5, Array.Empty<string>().ToHashSet());

            employeesCollection.Add(manager1, new[] { "manager1_1" }.ToHashSet());
            employeesCollection.Add(manager2, Array.Empty<string>().ToHashSet());
            employeesCollection.Add(manager3, Array.Empty<string>().ToHashSet());

            employeesCollection.Add(director1, Array.Empty<string>().ToHashSet());
            employeesCollection.Add(director2, Array.Empty<string>().ToHashSet());

            employeesCollection.Add(board1, Array.Empty<string>().ToHashSet());
            employeesCollection.Add(board2, Array.Empty<string>().ToHashSet());

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