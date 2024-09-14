using System;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models;
using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Services
{
    public class ChannelFilterTests
    {
        [Fact]
        public void ShouldFilter()
        {
            // Arrange
            var input = new[]
            {
                new Channel("channel1", "channelName11", new Team("team1", "TeamName1"), new[] { "user1", "user2" }),
                new Channel("channel2", "channelName12", new Team("team1", "TeamName1"), new[] { "user1" }),
                new Channel("channel1", "channelName21", new Team("team2", "TeamName1"), new[] { "user2" })
            };

            var filter = new EmployeeFilter(["user1"], Array.Empty<string>());

            // Act
            var result = new ChannelFilter(NullLogger<ChannelFilter>.Instance).Filter(input, filter);

            // Assert
            var expectedResult = new[] { input[0], input[1] };
            result.Should().BeEquivalentTo(expectedResult);
        }

    }
}