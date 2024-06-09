namespace NetworkPerspective.Sync.Infrastructure.SecretStorage.HashiCorpVault;

internal class HcpVaultConfig
{
    public string BaseUrl { get; set; }
    public string TestSecretName { get; set; }
    public string Token { get; set; }
    public string VaultRole { get; set; }
    public string MountPoint { get; set; }
}