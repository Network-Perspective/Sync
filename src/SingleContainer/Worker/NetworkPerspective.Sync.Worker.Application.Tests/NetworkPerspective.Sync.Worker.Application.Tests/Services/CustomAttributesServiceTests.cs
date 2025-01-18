using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using FluentAssertions;

using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Worker.Application.Tests.Services;

public class CustomAttributesServiceTests
{
    const string PropAttributeName1 = "prop_attribute_name_1";
    const string PropAttributeName2 = "prop_attribute_name_2";

    const string GroupAttributeName1 = "group_attribute_name_1";
    const string GroupAttributeName2 = "group_attribute_name_2";
    const string GroupAttributeName3 = "group_attribute_name_3";
    const string GroupAttributeName4 = "group_attribute_name_4";

    private static readonly IEnumerable<string> GroupAttributes = [GroupAttributeName1, GroupAttributeName2, GroupAttributeName3, GroupAttributeName4];
    private static readonly IEnumerable<string> PropAttributes = [PropAttributeName1, PropAttributeName2];

    private readonly CustomAttributesConfig _config = new(
        groupAttributes: GroupAttributes,
        propAttributes: PropAttributes,
        relationships: []);

    public class GetGroupsForHashedEmployee : CustomAttributesServiceTests
    {
        [Fact]
        public void ShouldCreateGroups()
        {
            // Arrange
            var customAttributesValues = new[]
            {
                CustomAttr.CreateMultiValue(GroupAttributeName1, "value1_1"),
                CustomAttr.CreateMultiValue(GroupAttributeName1, "value1_2"),
                CustomAttr.Create(GroupAttributeName2, "value2")
            };

            var expectedGroups = new[]
            {
                Group.Create($"{GroupAttributeName1}:value1_1", "value1_1", GroupAttributeName1),
                Group.Create($"{GroupAttributeName1}:value1_2", "value1_2", GroupAttributeName1),
                Group.Create($"{GroupAttributeName2}:value2", "value2", GroupAttributeName2),
            };

            var syncContextAccessor = CreateSyncContextAccessor(_config);

            var service = new CustomAttributesService(syncContextAccessor);

            // Act
            var actualGroups = service.GetGroupsForHashedEmployee(customAttributesValues);

            // Assert

            actualGroups.Should().BeEquivalentTo(expectedGroups);
        }
    }

    public class GetPropsForHashedEmployee : CustomAttributesServiceTests
    {
        [Fact]
        public void ShouldCreateProps()
        {
            // Arrange
            var customAttributesValues = new[]
            {
                CustomAttr.Create(PropAttributeName1, "value1"),
                CustomAttr.Create(PropAttributeName2, "value2"),
                CustomAttr.CreateMultiValue(GroupAttributeName1, "value3"),
                CustomAttr.CreateMultiValue(GroupAttributeName1, "value4"),
            };

            var expectedProps = new Dictionary<string, object>
            {
                { PropAttributeName1, "value1" },
                { PropAttributeName2, "value2" },
            };

            var syncContextAccessor = CreateSyncContextAccessor(_config);

            var service = new CustomAttributesService(syncContextAccessor);

            // Act
            var actualProps = service.GetPropsForHashedEmployee(customAttributesValues);

            // Assert
            actualProps.Should().BeEquivalentTo(expectedProps);
        }
    }

    public class GetPropsForEmployee : CustomAttributesServiceTests
    {
        [Fact]
        public void ShouldCreateProps()
        {
            // Arrange
            var customAttributesValues = new[]
            {
                CustomAttr.Create(PropAttributeName1, "value1"),
                CustomAttr.CreateMultiValue(GroupAttributeName1, "value2"),
                CustomAttr.CreateMultiValue(GroupAttributeName1, "value3"),
                CustomAttr.Create(GroupAttributeName2, "value4"),
                CustomAttr.CreateMultiValue(GroupAttributeName3, "value5")
            };

            var expectedProps = new Dictionary<string, object>
            {
                { GroupAttributeName1, new[] {"value2", "value3" } },
                { GroupAttributeName2, "value4"},
                { GroupAttributeName3, new[] {"value5"} }
            };

            var syncContextAccessor = CreateSyncContextAccessor(_config);

            var service = new CustomAttributesService(syncContextAccessor);

            // Act
            var actualProps = service.GetPropsForEmployee(customAttributesValues);

            // Assert
            actualProps.Should().BeEquivalentTo(expectedProps);
        }
    }

    private ISyncContextAccessor CreateSyncContextAccessor(CustomAttributesConfig customAttributesConfig)
    {
        var syncContext = new SyncContext(Guid.NewGuid(), "foo", new ConnectorConfig(EmployeeFilter.Empty, _config), ImmutableDictionary<string, string>.Empty, "bar".ToSecureString(), TimeRange.Empty);
        return new SyncContextAccessor { SyncContext = syncContext };
    }

}