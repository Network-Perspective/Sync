using FluentAssertions;

using Google.Apis.Admin.Directory.directory_v1.Data;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Criterias;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Tests.Criterias
{
    public class NonServiceUserCriteriaTests
    {
        private static readonly ILogger<NonServiceUserCriteria> Logger = NullLogger<NonServiceUserCriteria>.Instance;

        [Theory]
        [InlineData(null, "bar")]
        [InlineData("foo", null)]
        [InlineData(null, null)]
        [InlineData("", "bar")]
        public void ShouldFilterOutUsersWithoutRequiredProperties(string familyName, string givenName)
        {
            // Arrange
            var users = new[]
            {
                new User
                {
                    Name = new UserName
                    {
                        FamilyName = familyName,
                        GivenName = givenName
                    }
                }
            };

            // Act
            var filteredUsers = new NonServiceUserCriteria(Logger).MeetCriteria(users);

            // Assert
            filteredUsers.Should().BeEmpty();
        }

        [Fact]
        public void ShouldNotFilterOutUsersWithRequeredProperties()
        {
            // Arrange
            var users = new[]
            {
                new User
                {
                    Name = new UserName
                    {
                        FamilyName = "foo",
                        GivenName = "bar"
                    }
                }
            };

            // Act
            var filteredUsers = new NonServiceUserCriteria(Logger).MeetCriteria(users);

            // Assert
            filteredUsers.Should().BeEquivalentTo(users);
        }
    }
}