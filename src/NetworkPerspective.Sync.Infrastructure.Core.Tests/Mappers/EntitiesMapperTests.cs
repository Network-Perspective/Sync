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
            var manager = Employee.CreateExternal("baz");
            var props = new Dictionary<string, object>
            {
                {Employee.PropKeyName, "Alice Smith" },
                {Employee.PropKeyTeam, "Super Team" },
                {Employee.PropKeyDepartment, new[] {"IT", "DevOps"} }
            };
            var employee = Employee.CreateInternal("foo", "bar", "baz", Array.Empty<Group>(), props);

            // Act
            var result = EntitiesMapper.ToEntity(employee, manager, "test");

            // Assert
            result.Props.Should().NotContainKey(Employee.PropKeyName);
            result.Props.Should().NotContainKey(Employee.PropKeyTeam);
            result.Props.Should().NotContainKey(Employee.PropKeyDepartment);
        }
    }
}