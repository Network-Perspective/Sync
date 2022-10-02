﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using FluentAssertions;

using Google.Apis.Admin.Directory.directory_v1.Data;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Mappers;
using NetworkPerspective.Sync.Infrastructure.Google.Services;

using Xunit;

using Group = NetworkPerspective.Sync.Application.Domain.Employees.Group;

namespace NetworkPerspective.Sync.Infrastructure.Google.Tests.Mappers
{
    public class EmployeesMapperTests
    {
        [Fact]
        public void ShouldMapToEmployees()
        {
            // Arrange
            const string custom_attr_location_name = "Location";
            const string manager_email = "manager@baz.com";

            const string user_1_id = "432";
            const string user_1_name = "John Doe";
            const string user_1_email = "foo@baz.com";
            const string user_1_department_1 = "department_1_1";
            const string user_1_department_2 = "department_1_2";
            var user_1_custom_attr_location = "London";

            const string user_2_id = "876";
            const string user_2_name = "Alice Smith";
            const string user_2_email = "bar@baz.com";
            const string user_2_department_1 = "department_2_1";
            const string user_2_department_2 = "department_2_2";
            var user_2_custom_attr_location = "Berlin";

            var expectedEmployees = new EmployeeCollection(null);
            var expectedGroups1 = new[]
            {
                Group.Create($"Department:{user_1_department_1}", user_1_department_1, Group.DepartmentCatergory),
                Group.Create($"Department:{user_1_department_2}", user_1_department_2, Group.DepartmentCatergory),
                Group.Create("/", "/", Group.CompanyCatergory),
                Group.CreateWithParentId("/IT", "IT", "OrgUnitLevel1", "/"),
                Group.CreateWithParentId("/IT/Development", "Development", "OrgUnitLevel2", "/IT"),
                Group.CreateWithParentId("/IT/Development/Team1", "Team1", Group.TeamCatergory, "/IT/Development"),
            };
            var expectedProps1 = new Dictionary<string, object>
            {
                { "Name", user_1_name },
                { "CustomProps.Location", user_1_custom_attr_location }
            };
            var exectedRelations1 = new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, manager_email) });
            var expectedEmployee1 = Employee.CreateInternal(user_1_email, user_1_id, expectedGroups1, expectedProps1, exectedRelations1);
            expectedEmployees.Add(expectedEmployee1, ImmutableHashSet<string>.Empty);

            var expectedGroups2 = new[]
{
                Group.Create($"Department:{user_2_department_1}", user_2_department_1, Group.DepartmentCatergory),
                Group.Create($"Department:{user_2_department_2}", user_2_department_2, Group.DepartmentCatergory),
                Group.Create("/", "/", Group.CompanyCatergory),
                Group.CreateWithParentId("/IT", "IT", "OrgUnitLevel1", "/"),
                Group.CreateWithParentId("/IT/Development", "Development", "OrgUnitLevel2", "/IT"),
            };
            var expectedProps2 = new Dictionary<string, object>
            {
                { "Name", user_2_name },
                { "CustomProps.Location", user_2_custom_attr_location }
            };
            var expectedRelations2 = new RelationsCollection(new[] { Relation.Create(Employee.SupervisorRelationName, manager_email) });
            var expectedEmployee2 = Employee.CreateInternal(user_2_email, user_2_id, expectedGroups2, expectedProps2, expectedRelations2);
            expectedEmployees.Add(expectedEmployee2, ImmutableHashSet<string>.Empty);

            var users = new[]
            {
                new User
                {
                    Id = user_1_id,
                    Name = new UserName { FullName = user_1_name },
                    PrimaryEmail = user_1_email,
                    Emails = new[]
                    {
                        new UserEmail { Address = user_1_email },
                    },
                    Relations = new []
                    {
                        new UserRelation
                        {
                            Type = "manager",
                            Value = manager_email
                        }
                    },
                    Organizations = new[]
                    {
                        new UserOrganization
                        {
                            Department = user_1_department_1
                        },
                        new UserOrganization
                        {
                            Department = user_1_department_2
                        }
                    },
                    OrgUnitPath = "IT/Development/Team1",
                    CustomSchemas = new Dictionary<string, IDictionary<string, object>>
                    {
                        { "CustomProps", new Dictionary<string, object> { { custom_attr_location_name, user_1_custom_attr_location } } }
                    }
                },
                new User
                {
                    Id = user_2_id,
                    Name = new UserName { FullName = user_2_name },
                    PrimaryEmail = user_2_email,
                    Emails = new[]
                    {
                        new UserEmail { Address = user_2_email },
                    },
                    Relations = new []
                    {
                        new UserRelation
                        {
                            Type = "manager",
                            Value = manager_email
                        }
                    },
                    Organizations = new[]
                    {
                        new UserOrganization
                        {
                            Department = user_2_department_1
                        },
                        new UserOrganization
                        {
                            Department = user_2_department_2
                        }
                    },
                    OrgUnitPath = "IT/Development",
                    CustomSchemas = new Dictionary<string, IDictionary<string, object>>
                    {
                        { "CustomProps", new Dictionary<string, object> { { custom_attr_location_name, user_2_custom_attr_location } } }
                    }
                }
            };

            var customAttributesConfig = new CustomAttributesConfig(
                groupAttributes: new[] { "CustomProps.Location" },
                propAttributes: Array.Empty<string>(),
                relationships: Array.Empty<CustomAttributeRelationship>());

            var mapper = new EmployeesMapper(new CompanyStructureService(), new CustomAttributesService(customAttributesConfig));

            // Act
            var result = mapper.ToEmployees(users);

            // Assert
            result.GetAllInternal().Should().BeEquivalentTo(expectedEmployees.GetAllInternal());
        }
    }
}