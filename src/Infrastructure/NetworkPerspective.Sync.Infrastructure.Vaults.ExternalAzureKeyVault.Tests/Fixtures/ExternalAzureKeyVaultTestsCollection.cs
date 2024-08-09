using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.Vaults.ExternalAzureKeyVault.Tests.Fixtures;

[CollectionDefinition(Name)]
public class ExternalAzureKeyVaultTestsCollection : ICollectionFixture<ExternalAzureKeyVaultFixture>
{
    public const string Name = "ExternalAzureKeyVaultTestsCollection";
}