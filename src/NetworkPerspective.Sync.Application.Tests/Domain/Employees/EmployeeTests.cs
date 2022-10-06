using System.Linq;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Employees
{
    public class EmployeeTests
    {
        [Fact]
        public void ShouldHashSensitiveData()
        {
            // Arrange
            const string email = "email";
            const string sourceInternalId = "sourceInternalId";
            const string managerEmail = "managerEmail";
            const string groupId = "groupId";
            const string groupName = "groupName";
            const string groupCategory = "Project";
            const string groupParentId = "parentId";

            var group = Group.CreateWithParentId(groupId, groupName, groupCategory, groupParentId);

            HashFunction hashingFunction = (x) => $"{x}_hashed";

            var relation = new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, managerEmail) });
            var employeeId = EmployeeId.Create(email, sourceInternalId);
            var employee = Employee.CreateInternal(employeeId, new[] { group }, null, relation);

            // Act
            var hashedEmployee = employee.Hash(hashingFunction);

            // Assert
            hashedEmployee.Id.PrimaryId.Should().Be($"{email}_hashed");
            hashedEmployee.Id.DataSourceId.Should().Be($"{sourceInternalId}_hashed");
            hashedEmployee.ManagerEmail.Should().Be($"{managerEmail}_hashed");

            hashedEmployee.Groups.Single().Id.Should().Be($"{groupId}_hashed");
            hashedEmployee.Groups.Single().Name.Should().Be($"{groupName}");
            hashedEmployee.Groups.Single().Category.Should().Be(groupCategory);
            hashedEmployee.Groups.Single().ParentId.Should().Be($"{groupParentId}_hashed");

            hashedEmployee.Relations.GetTargetEmployeeEmail(Employee.SupervisorRelationName).Should().Be($"{managerEmail}_hashed");
        }

        [Fact]
        public void ShouldHaveTeam()
        {
            // Arrange
            const string expectedTeam = "/Marketing/SEO";

            var group = Group.CreateWithParentId(expectedTeam, "foo", Group.TeamCatergory, "/Marketing");

            // Act
            var employee = Employee.CreateInternal(EmployeeId.Create("bar", "baz"), new[] { group });

            // Assert
            employee.Props["Team"].Should().Be(expectedTeam);
        }

        [Fact]
        public void ShouldHaveEmptyTeamOnNotAssigned()
        {
            // Arrange
            var group = Group.CreateWithParentId("Marketing", "foo", Group.CompanyCatergory, "/");

            // Act
            var employee = Employee.CreateInternal(EmployeeId.Create("bar", "baz"), new[] { group });

            // Assert
            ((string)employee.Props["Team"]).Should().BeEmpty();
        }

        [Fact]
        public void ShouldReturnDepartments()
        {
            // Arrange
            var groups = new[]
            {
                Group.Create("Department:IT", "IT", "Department"),
                Group.Create("Department:Security", "Security", "Department"),
                Group.Create("Department:Administration", "Administration", "Department"),
            };

            // Act
            var employee = Employee.CreateInternal(EmployeeId.Create("bar", "baz"), groups);

            // Assert
            employee.Props["Department"].Should().BeEquivalentTo(new[] { "IT", "Security", "Administration" });
        }
    }
}