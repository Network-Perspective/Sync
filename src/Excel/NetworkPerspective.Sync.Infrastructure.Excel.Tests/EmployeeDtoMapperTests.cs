using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Infrastructure.Excel.Dtos;
using NetworkPerspective.Sync.Infrastructure.Excel.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Excel.Tests;

public class EmployeeDtoMapperTests
{

    [Fact]
    public void ToDomainEmployees_ShouldReturnEmptyList_WhenInputListIsEmpty()
    {
        // Arrange
        var dtos = new List<EmployeeDto>();
        var metadata = new SyncMetadataIncludesDto();
        var emailFilter = EmailFilter.Empty;

        // Act
        var result = dtos.ToDomainEmployees(metadata, emailFilter);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ToDomainEmployees_ShouldReturnExternalEmployees_WhenEmailsAreNotInternal()
    {
        // Arrange
        var dtos = new List<EmployeeDto>
        {
            new EmployeeDto { Email = "external1@example.com", EmployeeId = "1" },
            new EmployeeDto { Email = "external2@example.com", EmployeeId = "2" }
        };
        var metadata = new SyncMetadataIncludesDto();
        var emailFilter = new EmailFilter(new[] { "internal@example.com" }, new List<string>());

        // Act
        var result = dtos.ToDomainEmployees(metadata, emailFilter);

        // Assert
        Assert.All(result, employee => Assert.True(employee.IsExternal));
    }

    [Fact]
    public void ToDomainEmployees_ShouldReturnInternalEmployees_WhenEmailsAreInternal()
    {
        // Arrange
        var dtos = new List<EmployeeDto>
        {
            new EmployeeDto { Email = "internal1@example.com", EmployeeId = "1" },
            new EmployeeDto { Email = "internal2@example.com", EmployeeId = "2" }
        };
        var metadata = new SyncMetadataIncludesDto();
        var emailFilter =
            new EmailFilter(new[] { "internal1@example.com", "internal2@example.com" }, new List<string>());

        // Act
        var result = dtos.ToDomainEmployees(metadata, emailFilter);

        // Assert
        Assert.All(result, employee => Assert.False(employee.IsExternal));
    }

    [Fact]
    public void ToDomainEmployees_ShouldIgnorePropsNotIncludedInMetadata()
    {
        // Arrange
        var dtos = new List<EmployeeDto>
        {
            new EmployeeDto
            {
                Email = "internal1@example.com",
                EmployeeId = "1",
                Props = new List<EmployeePropDto> { new EmployeePropDto { Name = "Prop1", Value = "Value1" } }
            }
        };
        var metadata = new SyncMetadataIncludesDto { Props = new HashSet<string> { "Prop2" } };
        var emailFilter = EmailFilter.Empty;

        // Act
        var result = dtos.ToDomainEmployees(metadata, emailFilter);

        // Assert
        Assert.All(result, employee => Assert.Empty(employee.Props));
    }

    [Fact]
    public void ToDomainEmployees_ShouldIncludePropsIncludedInMetadata()
    {
        // Arrange
        var dtos = new List<EmployeeDto>
        {
            new EmployeeDto
            {
                Email = "internal1@example.com",
                EmployeeId = "1",
                Props = new List<EmployeePropDto> { new EmployeePropDto { Name = "Prop1", Value = "Value1" } }
            }
        };
        var metadata = new SyncMetadataIncludesDto { Props = new HashSet<string> { "Prop1" } };
        var emailFilter = EmailFilter.Empty;

        // Act
        var result = dtos.ToDomainEmployees(metadata, emailFilter);

        // Assert
        Assert.All(result, employee => Assert.Contains("Prop1", employee.Props.Keys));
    }

    [Fact]
    public void ToDomainEmployees_ShouldIgnoreGroupsNotIncludedInMetadata()
    {
        // Arrange
        var dtos = new List<EmployeeDto>
        {
            new EmployeeDto
            {
                Email = "internal1@example.com",
                EmployeeId = "1",
                Groups = new List<EmployeeGroupDto> { new EmployeeGroupDto { Category = "Group1", Name = "Name1" } }
            }
        };
        var metadata = new SyncMetadataIncludesDto { Groups = new HashSet<string> { "Group2" } };
        var emailFilter = EmailFilter.Empty;

        // Act
        var result = dtos.ToDomainEmployees(metadata, emailFilter);

        // Assert
        Assert.All(result, employee => Assert.Empty(employee.Groups));
    }

    [Fact]
    public void ToDomainEmployees_ShouldIncludeGroupsIncludedInMetadata()
    {
        // Arrange
        var dtos = new List<EmployeeDto>
        {
            new EmployeeDto
            {
                Email = "internal1@example.com",
                EmployeeId = "1",
                Groups = new List<EmployeeGroupDto> { new EmployeeGroupDto { Category = "Group1", Name = "Name1" } }
            }
        };
        var metadata = new SyncMetadataIncludesDto { Groups = new HashSet<string> { "Group1" } };
        var emailFilter = EmailFilter.Empty;

        // Act
        var result = dtos.ToDomainEmployees(metadata, emailFilter);

        // Assert
        Assert.All(result, employee => Assert.Contains(employee.Groups, group => group.Category == "Group1"));
    }


    [Fact]
    public void ToDomainEmployees_ShouldIgnoreRelationshipsNotIncludedInMetadata()
    {
        // Arrange
        var dtos = new List<EmployeeDto>
        {
            new EmployeeDto
            {
                Email = "internal1@example.com",
                EmployeeId = "1",
                Relationships = new List<EmployeeRelationshipDto> { new EmployeeRelationshipDto { RelationshipName = "Relationship1" } }
            }
        };
        var metadata = new SyncMetadataIncludesDto { Relationships = new HashSet<string> { "Relationship2" } };
        var emailFilter = EmailFilter.Empty;

        // Act
        var result = dtos.ToDomainEmployees(metadata, emailFilter);

        // Assert
        Assert.All(result, employee => Assert.Empty(employee.Relations));
    }

    [Fact]
    public void ToDomainEmployees_ShouldIncludeRelationshipsIncludedInMetadata()
    {
        // Arrange
        var dtos = new List<EmployeeDto>
        {
            new EmployeeDto
            {
                Email = "internal1@example.com",
                EmployeeId = "1",
                Relationships = new List<EmployeeRelationshipDto> { new EmployeeRelationshipDto
                    {
                        RelationshipName = "Relationship1",
                        Email = "internal2@example.com"
                    }
                }
            }
        };
        var metadata = new SyncMetadataIncludesDto { Relationships = new HashSet<string> { "Relationship1" } };
        var emailFilter = EmailFilter.Empty;

        // Act
        var result = dtos.ToDomainEmployees(metadata, emailFilter);

        // Assert
        Assert.All(result, employee => Assert.Contains(employee.Relations, relation => relation.Name == "Relationship1"));
    }


    [Fact]
    public void ToDomainEmployees_ShouldMapRelationshipsWithEmployeeId()
    {
        // Arrange
        var dtos = new List<EmployeeDto>
        {
            new EmployeeDto
            {
                Email = "internal1@example.com",
                EmployeeId = "1",
                Relationships = new List<EmployeeRelationshipDto> { new EmployeeRelationshipDto
                    {
                        RelationshipName = "Relationship1",
                        EmployeeId = "2"
                    }
                },
            },
            new EmployeeDto()
            {
                Email = "internal2@example.com",
                EmployeeId = "2"
            }
        };
        var metadata = new SyncMetadataIncludesDto { Relationships = new HashSet<string> { "Relationship1" } };
        var emailFilter = EmailFilter.Empty;

        // Act
        var result = dtos.ToDomainEmployees(metadata, emailFilter);

        // Assert
        var employee = result.Single(employee => employee.Id.PrimaryId == "internal1@example.com");

        Assert.Contains(employee.Relations, relation => relation.Name == "Relationship1");
        Assert.Contains(employee.Relations, relation => relation.TargetEmployeeEmail == "internal2@example.com");
    }

}