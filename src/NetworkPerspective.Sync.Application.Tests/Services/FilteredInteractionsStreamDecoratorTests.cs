using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Common.Tests.Factories;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Services
{
    public class FilteredInteractionsStreamDecoratorTests
    {
        public class SendAsync : FilteredInteractionsStreamDecoratorTests
        {
            [Fact]
            public async Task ShouldReturnActualSentInteractions()
            {
                // Arrange
                const int actuallySentInteractionsCount = 4;
                var interactions = InteractionFactory.CreateSet(10);
                var filterMock = new Mock<IInteractionsFilter>();
                filterMock
                    .Setup(x => x.Filter(interactions))
                    .Returns<ISet<Interaction>>(x => x.Take(actuallySentInteractionsCount).ToHashSet());

                var innerStream = new TestableInteractionStream();
                var filteredStream = new FilteredInteractionStreamDecorator(innerStream, filterMock.Object);

                // Act
                var result = await filteredStream.SendAsync(interactions);

                // Assert
                result.Should().Be(actuallySentInteractionsCount);
            }
        }
    }
}