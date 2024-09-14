using System;
using System.Security;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Vaults.ExternalAzureKeyVault.Tests.Fixtures;
using NetworkPerspective.Sync.Utils.Extensions;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.ExternalAzureKeyVault.Tests;

[Collection(ExternalAzureKeyVaultTestsCollection.Name)]
public class ExternalAzureKeyVaultClientTests : IAsyncLifetime
{
    private const string KeyNameTemplate = "test-key-{0}";
    private readonly string _keyName;

    private readonly IVault _client;

    public ExternalAzureKeyVaultClientTests(ExternalAzureKeyVaultFixture fixture)
    {
        _keyName = string.Format(KeyNameTemplate, Guid.NewGuid().ToString());
        _client = fixture.Client;
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

    public class GetSecret : ExternalAzureKeyVaultClientTests
    {
        public GetSecret(ExternalAzureKeyVaultFixture client) : base(client)
        { }


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

    public class SetSecret : ExternalAzureKeyVaultClientTests
    {
        public SetSecret(ExternalAzureKeyVaultFixture client) : base(client)
        { }

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

    public class RemoveSecret : ExternalAzureKeyVaultClientTests
    {
        public RemoveSecret(ExternalAzureKeyVaultFixture client) : base(client)
        { }

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