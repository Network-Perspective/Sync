using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.AzureKeyVault.Tests.Fixtures;

[CollectionDefinition(Name)]
public class AzureKeyVaultTestsCollection : ICollectionFixture<AzureKeyVaultFixture>
{
    public const string Name = "AzureKeyVaultTestsCollection";
}