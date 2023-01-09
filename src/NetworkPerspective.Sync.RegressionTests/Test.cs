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
            const string oldInteractionsDirPath = "C:\\Users\\MaciejKlimczuk\\Desktop\\temp\\dane\\old";
            const string newInteractionsDirPath = "C:\\Users\\MaciejKlimczuk\\Desktop\\temp\\dane\\new";

            var oldInteractions = await new InteractionsFromFileProvider(oldInteractionsDirPath).GetInteractionsAsync();
            var newInteractions = await new InteractionsFromFileProvider(newInteractionsDirPath).GetInteractionsAsync();

            var equalityComparer = new InteractionEqualityComparer();
            var comparer = new Comparer<HashedInteraction>(equalityComparer);

            // Act
            var result = comparer.Compare(oldInteractions, newInteractions);

            // Assert
        }
    }
}
