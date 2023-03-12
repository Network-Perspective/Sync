using System.Linq;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.Core;
using NetworkPerspective.Sync.RegressionTests.Interactions;
using NetworkPerspective.Sync.RegressionTests.Services;

using Xunit;
using Xunit.Abstractions;

namespace NetworkPerspective.Sync.RegressionTests
{
    public class RegressionTests
    {
        private readonly ITestOutputHelper _output;

        public RegressionTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldContainTheSameInteractionsLikeOldVersion()
        {
            // Arrange
            const string oldInteractionsDirPath = "D:\\data\\old\\interactions";
            const string newInteractionsDirPath = "D:\\data\\new\\interactions";

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

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldGenerateTheSameInteractionsOnMeetingForAllEmployees()
        {
            // Arrange
            const string interactionsDirPath = "D:\\data\\new\\interactions";

            var interactions = await new InteractionsFromFileProvider(interactionsDirPath).GetInteractionsAsync();

            var meetingsInteractions = interactions
                .Where(x => x.Label.Contains(HashedInteractionLabel.Meeting))
                .GroupBy(x => x.EventId);

            // Act
            _output.WriteLine("Meeting Id,User Id,Interactions count,Reference User Id, Reference interactions count");

            foreach (var singleMeetingInteractions in meetingsInteractions)
            {
                var usersMeetingInteractions = singleMeetingInteractions.GroupBy(x => x.SourceIds, VertexIdEqualityComparer.Instance);

                var referenceUser = usersMeetingInteractions.First(x => x.Key["Email"] != "external");

                foreach (var singleUserMeetingInteractions in usersMeetingInteractions.Where(x => x.Key["Email"] != "external"))
                {
                    if(singleUserMeetingInteractions.Count() != referenceUser.Count())
                    {
                        var output = string.Format("{0},{1},{2},{3},{4}", singleMeetingInteractions.Key, singleUserMeetingInteractions.Key["Email"], singleUserMeetingInteractions.Count().ToString(), referenceUser.Key["Email"], referenceUser.Count());
                        _output.WriteLine(output);
                    }
                }
            }
        }
    }
}