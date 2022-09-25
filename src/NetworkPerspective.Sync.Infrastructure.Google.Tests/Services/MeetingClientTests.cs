using System;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Common.Tests.Extensions;
using NetworkPerspective.Sync.Infrastructure.Google.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Google.Tests.Services
{
    public class MeetingClientTests : IClassFixture<GoogleClientFixture>
    {
        private readonly GoogleClientFixture _googleClientFixture;

        public MeetingClientTests(GoogleClientFixture googleClientFixture)
        {
            _googleClientFixture = googleClientFixture;
        }

        [Theory]
        [InlineData("nptestuser12@worksmartona.com")]
        [InlineData("john@worksmartona.com")]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldGetNonEmptyUserCollection(string email)
        {
            // Arrange
            var googleConfig = new GoogleConfig
            {
                ApplicationName = "gmail_app",
            };

            var client = new MeetingClient(Mock.Of<ITasksStatusesCache>(), Options.Create(googleConfig), NullLogger<MeetingClient>.Instance);
            var timeRange = new TimeRange(DateTime.MinValue, DateTime.MaxValue);

            var emailLookuptable = new EmployeeCollection(null)
                .Add(email);
            var interactionFactory = new InteractionFactory((x) => $"{x}_hashed", emailLookuptable, new Clock());

            // Act
            var result = await client.GetInteractionsAsync(email, timeRange, _googleClientFixture.Credential, interactionFactory);

            // Assert
            result.Should().NotBeNullOrEmpty();
        }
    }
}