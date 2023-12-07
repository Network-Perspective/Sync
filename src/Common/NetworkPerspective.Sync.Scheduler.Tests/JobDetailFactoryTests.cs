using System;

using FluentAssertions;

using Quartz;

using Xunit;

namespace NetworkPerspective.Sync.Scheduler.Tests
{
    public class JobDetailFactoryTests
    {
        [Fact]
        public void ShouldHaveNetworkIdIdentity()
        {
            // Arrange
            var jobKey = new JobKey(Guid.NewGuid().ToString());
            var factory = new JobDetailFactory<TestableJob>();

            // Act
            var result = factory.Create(jobKey);

            // Assert
            result.Key.Should().Be(jobKey);
        }

        [Fact]
        public void ShouldBeDurable()
        {
            // Arrange
            var jobKey = new JobKey(Guid.NewGuid().ToString());
            var factory = new JobDetailFactory<TestableJob>();

            // Act
            var result = factory.Create(jobKey);

            // Assert
            result.Durable.Should().Be(true);
        }
    }
}