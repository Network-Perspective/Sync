using System;
using System.Collections.Generic;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Services;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class CustomAttributesServiceTests
    {
        const string propAttributeName1 = "prop_attribute_name_1";
        const string propAttributeName2 = "prop_attribute_name_2";

        const string groupAttributeName1 = "group_attribute_name_1";
        const string groupAttributeName2 = "group_attribute_name_2";
        const string groupAttributeName3 = "group_attribute_name_3";
        const string groupAttributeName4 = "group_attribute_name_4";

        static readonly IEnumerable<string> groupAttributes = new[] { groupAttributeName1, groupAttributeName2, groupAttributeName3, groupAttributeName4 };
        static readonly IEnumerable<string> propAttributes = new[] { propAttributeName1, propAttributeName2 };

        readonly CustomAttributesConfig config = new CustomAttributesConfig(
            groupAttributes: groupAttributes,
            propAttributes: propAttributes,
            relationships: Array.Empty<CustomAttributeRelationship>());

        public class GetGroupsForHashedEmployee : CustomAttributesServiceTests
        {
            [Fact]
            public void ShouldCreateGroups()
            {
                // Arrange
                var customAttributesValues = new[]
                {
                    CustomAttr.CreateMultiValue(groupAttributeName1, "value1_1"),
                    CustomAttr.CreateMultiValue(groupAttributeName1, "value1_2"),
                    CustomAttr.Create(groupAttributeName2, "value2")
                };

                var expectedGroups = new[]
                {
                    Group.Create($"{groupAttributeName1}:value1_1", "value1_1", groupAttributeName1),
                    Group.Create($"{groupAttributeName1}:value1_2", "value1_2", groupAttributeName1),
                    Group.Create($"{groupAttributeName2}:value2", "value2", groupAttributeName2),
                };

                var service = new CustomAttributesService(config);

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
                    CustomAttr.Create(propAttributeName1, "value1"),
                    CustomAttr.Create(propAttributeName2, "value2"),
                    CustomAttr.CreateMultiValue(groupAttributeName1, "value3"),
                    CustomAttr.CreateMultiValue(groupAttributeName1, "value4"),
                };

                var expectedProps = new Dictionary<string, object>
                {
                    { propAttributeName1, "value1" },
                    { propAttributeName2, "value2" },
                };

                var service = new CustomAttributesService(config);

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
                    CustomAttr.Create(propAttributeName1, "value1"),
                    CustomAttr.CreateMultiValue(groupAttributeName1, "value2"),
                    CustomAttr.CreateMultiValue(groupAttributeName1, "value3"),
                    CustomAttr.Create(groupAttributeName2, "value4"),
                    CustomAttr.CreateMultiValue(groupAttributeName3, "value5")
                };

                var expectedProps = new Dictionary<string, object>
                {
                    { groupAttributeName1, new[] {"value2", "value3" } },
                    { groupAttributeName2, "value4"},
                    { groupAttributeName3, new[] {"value5"} }
                };

                var service = new CustomAttributesService(config);

                // Act
                var actualProps = service.GetPropsForEmployee(customAttributesValues);

                // Assert
                actualProps.Should().BeEquivalentTo(expectedProps);
            }
        }

    }
}