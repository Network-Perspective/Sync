using System;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Models;
using NetworkPerspective.Sync.Infrastructure.Microsoft.Services;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Tests.Services
{
    public class ChannelFilterTests
    {
        [Fact]
        public void ShouldFilter()
        {
            // Arrange
            var input = new[]
            {
                new Channel(ChannelIdentifier.Create("team1", "channel1"), new[] { "user1", "user2" }),
                new Channel(ChannelIdentifier.Create("team1", "channel2"), new[] { "user1" }),
                new Channel(ChannelIdentifier.Create("team2", "channel1"), new[] { "user2" })
            };

            var filter = new EmailFilter(new[] { "user1" }, Array.Empty<string>());

            // Act
            var result = new ChannelFilter(NullLogger<ChannelFilter>.Instance).Filter(input, filter);

            // Assert
            var expectedResult = new[] { input[0], input[1] };
            result.Should().BeEquivalentTo(expectedResult);
        }

    }
}