using System;
using System.Collections.Generic;

using FluentAssertions;

using Google.Apis.Admin.Directory.directory_v1.Data;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Extensions;

using Newtonsoft.Json.Linq;

using Xunit;

using DomainGroup = NetworkPerspective.Sync.Application.Domain.Employees.Group;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests.Extensions
{
    public class UserExtensionsTests
    {
        [Fact]
        public void ShouldReturnFullName()
        {
            // Arrange
            var user = new User
            {
                Name = new UserName
                {
                    FullName = "John Doe"
                }
            };

            // Act
            var fullName = user.GetFullName();

            // Assert
            fullName.Should().Be("John Doe");
        }


        [Fact]
        public void GetGroupsShouldReturnAllGroups()
        {
            // Arrange
            var expectedGroups = new[]
            {
                DomainGroup.Create("Department:IT", "IT", "Department"),
            };

            var user = new User
            {
                Organizations = new[]
                {
                    new UserOrganization
                    {
                        Department = "IT"
                    }
                }
            };

            // Act
            var actualGroups = user.GetDepartmentGroups();

            // Assert
            actualGroups.Should().BeEquivalentTo(expectedGroups);
        }

        [Fact]
        public void ShouldCreateCorrectGroupIds()
        {
            // Arrange
            var expectedGroupsIds = new[] { "/", "/IT", "/IT/Apps", "/IT/Apps/Connector" };

            var user = new User
            {
                OrgUnitPath = "/IT/Apps/Connector"
            };

            // Act
            var result = user.GetOrganizationGroupsIds();

            // Assert
            result.Should().BeEquivalentTo(expectedGroupsIds);
        }

        public class GetCustomAttr : UserExtensionsTests
        {
            [Fact]
            public void ShouldReturnCustomAttrList()
            {
                // Arrange
                var expectedCustomAttr = new[]
                {
                    CustomAttr.CreateMultiValue("Test.Role", "HR partner"),
                    CustomAttr.CreateMultiValue("Test.Role", "Team lead"),
                    CustomAttr.Create("Test.Formal_Group", "GAME2/ENCOUNTERS DESIGN/GAMEPLAY DESIGN"),
                    CustomAttr.Create("Test.Employment_Date", new DateTime(2022, 08, 17))
                };

                var user = new User
                {
                    CustomSchemas = new Dictionary<string, IDictionary<string, object>>
                    {
                        {
                            "Test", new Dictionary<string, object>
                                    {
                                        { "Role", JArray.FromObject(new []
                                                    {
                                                        new { Value = "HR partner"},
                                                        new { Value = "Team lead"}
                                                    })
                                        },
                                        { "Formal_Group", "GAME2/ENCOUNTERS DESIGN/GAMEPLAY DESIGN" },
                                        { "Employment_Date", new DateTime(2022, 08, 17) }
                                    }
                        }
                    }
                };

                // Act
                var result = user.GetCustomAttrs();

                // Assert
                result.Should().BeEquivalentTo(expectedCustomAttr);
            }
        }
    }
}