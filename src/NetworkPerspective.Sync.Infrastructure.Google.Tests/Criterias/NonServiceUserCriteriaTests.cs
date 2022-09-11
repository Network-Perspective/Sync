using FluentAssertions;

using Google.Apis.Admin.Directory.directory_v1.Data;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Infrastructure.Google.Criterias;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Google.Tests.Criterias
{
    public class NonServiceUserCriteriaTests
    {
        private static readonly ILogger<NonServiceUserCriteria> Logger = NullLogger<NonServiceUserCriteria>.Instance;

        [Theory]
        [InlineData(null, "bar", "baz")]
        [InlineData("foo", null, "baz")]
        //[InlineData("foo", "bar", null)] To be uncommented once we know rule to filter out service accounts
        [InlineData(null, null, "baz")]
        [InlineData(null, "bar", null)]
        [InlineData("foo", null, null)]
        [InlineData(null, null, null)]
        [InlineData("", "bar", "baz")]
        public void ShouldFilterOutUsersWithoutRequiredProperties(string familyName, string givenName, string title)
        {
            // Arrange
            var users = new[]
            {
                new User
                {
                    Name = new UserName
                    {
                        FamilyName =familyName,
                        GivenName = givenName
                    },
                    Organizations = new[]
                    {
                        new UserOrganization
                        {
                            Title = title,
                        }
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
                    },
                    Organizations = new[]
                    {
                        new UserOrganization
                        {
                            Title = "baz",
                        }
                    }
                }
            };

            // Act
            var filteredUsers = new NonServiceUserCriteria(Logger).MeetCriteria(users);

            // Assert
            filteredUsers.Should().BeEquivalentTo(users);
        }

        [Fact(Skip = "We need to adjust the rule to reality, once we know how to filter out service accounts")]
        public void ShouldFilterOutUsersWithoutAnyOrganization()
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
                    },
                    Organizations = null
                }
            };

            // Act
            var filteredUsers = new NonServiceUserCriteria(Logger).MeetCriteria(users);

            // Assert
            filteredUsers.Should().BeEmpty();
        }

    }
}