using System;
using System.Security;
using System.Threading.Tasks;

using FluentAssertions;

using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Vaults.GoogleSecretManager.Tests.Fixtures;
using NetworkPerspective.Sync.Utils.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.GoogleSecretManager.Tests;

public class GoogleSecretManagerClientTests : GoogleSecretManagerTestsCollection, IAsyncLifetime
{
    private const string KeyNameTemplate = "test-key-{0}";

    private readonly string _keyName;

    private readonly GoogleSecretManagerClient _client;

    public GoogleSecretManagerClientTests()
    {
        _keyName = string.Format(KeyNameTemplate, Guid.NewGuid().ToString());
        var config = Options.Create(new GoogleSecretManagerConfig { ProjectId = "test-connectors-412712" });

        _client = new GoogleSecretManagerClient(config, NullLogger<GoogleSecretManagerClient>.Instance);
    }

    public Task InitializeAsync()
    => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        try
        {
            await _client.RemoveSecretAsync(_keyName);
        }
        catch (Exception)
        { }
    }

    public class GetSecret : GoogleSecretManagerClientTests
    {
        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldReturnStoredKey()
        {
            // Arrange
            const string keyValue = "foo";
            await _client.SetSecretAsync(_keyName, keyValue.ToSecureString());

            // Act
            var result = await _client.GetSecretAsync(_keyName);

            // Assert
            result.ToSystemString().Should().Be(keyValue);
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldThrowIfSecretDoesntExist()
        {
            // Arrange
            Func<Task<SecureString>> func = () => _client.GetSecretAsync(_keyName);

            // Act Assert
            await func.Should().ThrowAsync<VaultException>();
        }
    }

    public class SetSecret : GoogleSecretManagerClientTests
    {
        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldOverrideExistingKey()
        {
            // Arrange
            const string oldKeyValue = "foo";
            const string newKeyValue = "bar";

            await _client.SetSecretAsync(_keyName, oldKeyValue.ToSecureString());

            // Act
            await _client.SetSecretAsync(_keyName, newKeyValue.ToSecureString());

            // Assert
            var result = await _client.GetSecretAsync(_keyName);
            result.ToSystemString().Should().Be(newKeyValue);
        }
    }

    public class RemoveSecret : GoogleSecretManagerClientTests
    {
        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldRemoveSecret()
        {
            // Arrange
            const string keyValue = "foo";

            await _client.SetSecretAsync(_keyName, keyValue.ToSecureString());

            // Act
            await _client.RemoveSecretAsync(_keyName);

            // Assert
            Func<Task<SecureString>> func = () => _client.GetSecretAsync(_keyName);

            await func.Should().ThrowAsync<VaultException>();
        }

        [Fact]
        [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
        public async Task ShouldThrowOnNonExistingSecret()
        {
            // Arrange
            Func<Task> func = () => _client.RemoveSecretAsync(_keyName);

            // Act Assert
            await func.Should().ThrowAsync<VaultException>();
        }
    }
}