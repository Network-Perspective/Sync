using System;
using System.Security;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage.Exceptions;
using NetworkPerspective.Sync.Common.Tests;
using NetworkPerspective.Sync.Infrastructure.SecretStorage.Tests.Fixtures;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage.Tests
{
    [Collection(AzureKeyVaultTestsCollection.Name)]
    public class ExternalAzureKeyVaultClientTests : IAsyncLifetime
    {
        private const string KeyNameTemplate = "test-key-{0}";
        private readonly string _keyName;

        private readonly ExternalAzureKeyVaultClient _client;

        public ExternalAzureKeyVaultClientTests(AzureKeyVaultFixture fixture)
        {
            _keyName = string.Format(KeyNameTemplate, Guid.NewGuid().ToString());
            _client = fixture.ExternalClient;
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
            public GetSecret(AzureKeyVaultFixture client) : base(client)
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
                await func.Should().ThrowAsync<SecretStorageException>();
            }
        }

        public class SetSecret : ExternalAzureKeyVaultClientTests
        {
            public SetSecret(AzureKeyVaultFixture client) : base(client)
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
            public RemoveSecret(AzureKeyVaultFixture client) : base(client)
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

                await func.Should().ThrowAsync<SecretStorageException>();
            }

            [Fact]
            [Trait(TestsConsts.TraitSkipInCiName, TestsConsts.TraitRequiredTrue)]
            public async Task ShouldThrowOnNonExistingSecret()
            {
                // Arrange
                Func<Task> func = () => _client.RemoveSecretAsync(_keyName);

                // Act Assert
                await func.Should().ThrowAsync<SecretStorageException>();
            }
        }

    }
}