using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.SecretStorage.Tests.Fixtures
{
    [CollectionDefinition(Name)]
    public class AzureKeyVaultTestsCollection : ICollectionFixture<AzureKeyVaultFixture>
    {
        public const string Name = "AzureKeyVaultTestsCollection";
    }
}