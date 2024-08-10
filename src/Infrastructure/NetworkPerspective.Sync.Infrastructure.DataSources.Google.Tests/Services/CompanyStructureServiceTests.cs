using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Infrastructure.Google.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSource.Google.Tests.Services
{
    public class CompanyStructureServiceTests
    {
        [Fact]
        public void ShouldCreateSetOfGroups()
        {
            // Arrange
            var expectedGroups = new[]
            {
                Group.Create("/", "/", "OrgUnitCompany"),
                Group.CreateWithParentId("/IT", "IT", "OrgUnitLevel1", "/"),
                Group.CreateWithParentId("/IT/Apps", "Apps", "OrgUnitLevel2", "/IT"),
                Group.CreateWithParentId("/IT/Apps/Connectors", "Connectors", "OrgUnitTeam", "/IT/Apps")
            };

            var orgPaths = new[] { "/IT/Apps/Connectors", "/IT/Apps/Connectors", "/IT/Apps", "/IT/Apps" };

            // Act
            var result = new CompanyStructureService().CreateGroups(orgPaths);

            // Assert
            result.Should().BeEquivalentTo(expectedGroups);
        }
    }
}