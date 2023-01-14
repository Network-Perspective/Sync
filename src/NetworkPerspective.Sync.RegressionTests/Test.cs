using System.Linq;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.Core;
using NetworkPerspective.Sync.RegressionTests.Interactions;
using NetworkPerspective.Sync.RegressionTests.Services;

using Xunit;

namespace NetworkPerspective.Sync.RegressionTests
{
    public class Test
    {
        [Fact]
        public async Task Should()
        {
            // Arrange
            const string oldInteractionsDirPath = "D:\\dane\\old\\interactions";
            const string newInteractionsDirPath = "D:\\dane\\new\\interactions";

            var oldInteractions = await new InteractionsFromFileProvider(oldInteractionsDirPath).GetInteractionsAsync();
            var newInteractions = await new InteractionsFromFileProvider(newInteractionsDirPath).GetInteractionsAsync();


            var equalityComparer = new HashedInteractionEqualityComparer();
            var comparer = new Comparer<HashedInteraction>(equalityComparer);

            var newDist = newInteractions.Distinct(equalityComparer).ToList();

            var overlaping = newInteractions.Except(newDist).ToList();

            // Act
            var result = comparer.Compare(oldInteractions, newInteractions);

            // Assert
        }
    }
}