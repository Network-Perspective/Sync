using System;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Moq;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Common.Tests.Extensions;
using NetworkPerspective.Sync.Infrastructure.Google.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Google.Tests.Services
{
    public class EmailClientTests : IClassFixture<GoogleClientFixture>
    {
        private readonly GoogleClientFixture _googleClientFixture;

        public EmailClientTests(GoogleClientFixture googleClientFixture)
        {
            _googleClientFixture = googleClientFixture;
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldReturnNonEmptyEmailCollection()
        {
            // Arrange
            const string existingEmail = "nptestuser12@worksmartona.com";

            var googleConfig = new GoogleConfig
            {
                ApplicationName = "gmail_app",
                MaxMessagesPerUserDaily = 1000,
                SyncOverlapInMinutes = 0
            };

            var clock = new Clock();

            var emailClient = new EmailClient(Mock.Of<IStatusLogger>(), Options.Create(googleConfig), NullLoggerFactory.Instance, clock);

            var emailLookuptable = new EmployeeCollection(null)
                .Add(existingEmail);
            var interactionFactory = new InteractionFactory((x) => $"{x}_hashed", emailLookuptable, clock);

            // Act
            var result = await emailClient.GetInteractionsAsync(Guid.NewGuid(), new[] { Employee.CreateInternal(existingEmail, existingEmail, string.Empty, Array.Empty<Group>()) }, new DateTime(2021, 11, 01), _googleClientFixture.Credential, interactionFactory);

            // Assert
            result.Should().NotBeNullOrEmpty();
        }
    }
}