﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;

using FluentAssertions;

using Google.Apis.Admin.Directory.directory_v1.Data;

using NetworkPerspective.Sync.Common.Tests.Services;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Mappers;
using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services;
using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Services;

using Xunit;

using Group = NetworkPerspective.Sync.Worker.Application.Domain.Employees.Group;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests.Mappers;

public class HashedEmployeesMapperTests
{
    [Fact]
    public void ShouldMapToHashedEmployees()
    {
        // Arrange
        const string manager_email = "manager@baz.com";

        const string user_1_id = "432";
        const string user_1_name = "John Doe";
        const string user_1_email = "foo@baz.com";
        const string user_1_department_1 = "department_1_1";
        const string user_1_department_2 = "department_1_2";

        const string user_2_id = "876";
        const string user_2_name = "Alice Smith";
        const string user_2_email = "bar@baz.com";
        const string user_2_department_1 = "department_2_1";
        const string user_2_department_2 = "department_2_2";

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
        };
        var exectedRelations1 = new RelationsCollection(new[] { Relation.Create(Relation.SupervisorRelationName, manager_email) });
        var expectedEmployeeId1 = EmployeeId.CreateWithAliases(user_1_email, user_1_id, new[] { user_1_email }, EmployeeFilter.Empty);
        var expectedEmployee1 = Employee.CreateInternal(expectedEmployeeId1, expectedGroups1, expectedProps1, exectedRelations1);

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
        };
        var expectedRelations2 = new RelationsCollection([Relation.Create(Relation.SupervisorRelationName, manager_email)]);
        var expectedEmployeeId2 = EmployeeId.CreateWithAliases(user_2_email, user_2_id, [user_2_email], EmployeeFilter.Empty);
        var expectedEmployee2 = Employee.CreateInternal(expectedEmployeeId2, expectedGroups2, expectedProps2, expectedRelations2);

        var expectedEmployees = new EmployeeCollection([expectedEmployee1, expectedEmployee2], x => $"{x}_hashed");

        var users = new[]
        {
            new User
            {
                Id = user_1_id,
                Name = new UserName { FullName = user_1_name },
                CreationTimeDateTimeOffset = new DateTime(2022, 01, 03),
                PrimaryEmail = user_1_email,
                Emails =
                [
                    new UserEmail { Address = user_1_email },
                ],
                Relations =
                [
                    new UserRelation
                    {
                        Type = "manager",
                        Value = manager_email
                    }
                ],
                Organizations =
                [
                    new UserOrganization
                    {
                        Department = user_1_department_1
                    },
                    new UserOrganization
                    {
                        Department = user_1_department_2
                    }
                ],
                OrgUnitPath = "IT/Development/Team1"
            },
            new User
            {
                Id = user_2_id,
                Name = new UserName { FullName = user_2_name },
                PrimaryEmail = user_2_email,
                Emails =
                [
                    new UserEmail { Address = user_2_email },
                ],
                Relations =
                [
                    new UserRelation
                    {
                        Type = "manager",
                        Value = manager_email
                    }
                ],
                Organizations =
                [
                    new UserOrganization
                    {
                        Department = user_2_department_1
                    },
                    new UserOrganization
                    {
                        Department = user_2_department_2
                    }
                ],
                OrgUnitPath = "IT/Development"
            }
        };

        var syncContext = new SyncContext(Guid.NewGuid(), "foo", ConnectorConfig.Empty, ImmutableDictionary<string, string>.Empty, "bar".ToSecureString(), TimeRange.Empty);
        var syncContextAccessor = new SyncContextAccessor { SyncContext = syncContext };

        var mapper = new HashedEmployeesMapper(
            new CompanyStructureService(),
            new CustomAttributesService(syncContextAccessor),
            syncContextAccessor,
            new TestableHashingService(x => $"{x}_hashed")
        );

        // Act
        var result = mapper.ToEmployees(users, EmployeePropsSource.Empty);

        // Assert
        result.GetAllInternal().Should().BeEquivalentTo(expectedEmployees.GetAllInternal());
    }
}