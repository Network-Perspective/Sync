using System;
using System.Collections.Generic;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Infrastructure.Core.Mappers;

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
            var employee = Employee.CreateInternal("foo", "bar", Array.Empty<Group>(), props);

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

            var employee = Employee.CreateInternal("foo", "bar", Array.Empty<Group>(), props);

            // Act
            var result = UsersMapper.ToUser(employee, "test");

            // Assert
            result.Props.Should().NotContainKey(Employee.PropKeyCreationTime);
        }
    }
}