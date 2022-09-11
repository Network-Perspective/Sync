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
            lookupTable.Add(Employee.CreateInternal(mainEmail, id, string.Empty, Array.Empty<Group>()), new[] { alias_1, alias_2 }.ToHashSet());

            // Act
            var employee1 = lookupTable.Find(mainEmail);
            var employee2 = lookupTable.Find(alias_1);
            var employee3 = lookupTable.Find(alias_2);
            var employee4 = lookupTable.Find(id);

            // Assert
            employee1.Email.Should().Be(mainEmail);
            employee2.Email.Should().Be(mainEmail);
            employee3.Email.Should().Be(mainEmail);
            employee4.Email.Should().Be(mainEmail);
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
            email.Email.Should().Be(unkwnownAlias);
        }

        [Fact]
        public void ShouldCorrectlyEvaluteHierarchy()
        {
            // Arrange
            var ic1 = Employee.CreateInternal("ic1", string.Empty, "manager1", Array.Empty<Group>());
            var ic2 = Employee.CreateInternal("ic2", string.Empty, "manager1_1", Array.Empty<Group>());
            var ic3 = Employee.CreateInternal("ic3", string.Empty, "manager2", Array.Empty<Group>());
            var ic4 = Employee.CreateInternal("ic4", string.Empty, "manager3", Array.Empty<Group>());
            var ic5 = Employee.CreateInternal("ic5", string.Empty, "director1", Array.Empty<Group>());

            var manager1 = Employee.CreateInternal("manager1", string.Empty, "director1", Array.Empty<Group>());
            var manager2 = Employee.CreateInternal("manager2", string.Empty, "director2", Array.Empty<Group>());
            var manager3 = Employee.CreateInternal("manager3", string.Empty, "board2", Array.Empty<Group>());

            var director1 = Employee.CreateInternal("director1", string.Empty, "board1", Array.Empty<Group>());
            var director2 = Employee.CreateInternal("director2", string.Empty, "board1", Array.Empty<Group>());

            var board1 = Employee.CreateInternal("board1", string.Empty, null, Array.Empty<Group>());
            var board2 = Employee.CreateInternal("board2", string.Empty, null, Array.Empty<Group>());

            var employeesCollection = new EmployeeCollection(null);
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

            // Act
            employeesCollection.EvaluateHierarchy();

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