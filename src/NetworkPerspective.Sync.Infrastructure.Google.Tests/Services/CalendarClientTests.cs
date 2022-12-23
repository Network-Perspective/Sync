using System;
using System.Collections.Generic;
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
    public class CalendarClientTests : IClassFixture<GoogleClientFixture>
    {
        private readonly GoogleClientFixture _googleClientFixture;

        public CalendarClientTests(GoogleClientFixture googleClientFixture)
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

            var client = new CalendarClient(Mock.Of<ITasksStatusesCache>(), Options.Create(googleConfig), NullLogger<CalendarClient>.Instance);
            var timeRange = new TimeRange(DateTime.UtcNow.AddDays(-7), DateTime.UtcNow);

            var employees = new List<Employee>()
                .Add(email);

            var employeesCollection = new EmployeeCollection(employees, null);

            var interactionFactory = new MeetingInteractionFactory((x) => $"{x}_hashed", employeesCollection);

            // Act
            var result = await client.GetInteractionsAsync(Guid.NewGuid(), employeesCollection.GetAllInternal(), timeRange, _googleClientFixture.Credential, interactionFactory);

            // Assert
            result.Should().NotBeNullOrEmpty();
        }
    }
}