﻿using System;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.Google.Criterias;
using NetworkPerspective.Sync.Infrastructure.Google.Services;
using NetworkPerspective.Sync.Infrastructure.Google.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Google.Tests.Services
{
    public class MemberProfilesClientTests : IClassFixture<GoogleClientFixture>
    {
        private readonly GoogleClientFixture _googleClientFixture;

        public MemberProfilesClientTests(GoogleClientFixture googleClientFixture)
        {
            _googleClientFixture = googleClientFixture;
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldGetNonEmptyUserCollection()
        {
            // Arrange
            var network = Network<GoogleNetworkProperties>.Create(Guid.NewGuid(), new GoogleNetworkProperties("nptestuser12@worksmartona.com", null), DateTime.UtcNow);
            var googleConfig = new GoogleConfig
            {
                ApplicationName = "gmail_app",
            };

            var client = new UsersClient(Options.Create(googleConfig), Array.Empty<ICriteria>(), NullLogger<UsersClient>.Instance);

            // Act
            var result = await client.GetUsersAsync(network, NetworkConfig.Empty, _googleClientFixture.Credential);

            // Assert
            result.Should().NotBeNullOrEmpty();
        }

    }
}