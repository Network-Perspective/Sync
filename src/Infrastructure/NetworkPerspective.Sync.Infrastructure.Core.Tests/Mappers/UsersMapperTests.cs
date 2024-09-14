using System;
using System.Collections.Generic;

using FluentAssertions;

using NetworkPerspective.Sync.Infrastructure.Core.Mappers;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Core.Tests.Mappers
{
    public class UsersMapperTests
    {
        [Fact]
        public void ShouldMapHierarchy()
        {
            // Arrange
            var props = new Dictionary<string, object>
            {
                { Employee.PropKeyHierarchy, EmployeeHierarchy.IndividualContributor },
            };
            var employeeId = EmployeeId.Create("foo", "bar");
            var employee = Employee.CreateInternal(employeeId, Array.Empty<Group>(), props);

            // Act
            var result = UsersMapper.ToUser(employee, "test");

            // Assert
            result.Props[Employee.PropKeyHierarchy].Should().Be("Individual contributor");
        }

        [Fact]
        public void ShouldNotMapCreationTime()
        {
            // Arrange
            var props = new Dictionary<string, object>
            {
                { Employee.PropKeyCreationTime, DateTime.UtcNow },
            };

            var employeeId = EmployeeId.Create("foo", "bar");
            var employee = Employee.CreateInternal(employeeId, Array.Empty<Group>(), props);

            // Act
            var result = UsersMapper.ToUser(employee, "test");

            // Assert
            result.Props.Should().NotContainKey(Employee.PropKeyCreationTime);
        }
    }
}