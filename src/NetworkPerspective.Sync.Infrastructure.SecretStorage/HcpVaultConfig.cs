namespace NetworkPerspective.Sync.Infrastructure.SecretStorage
{
    public class HcpVaultConfig
    {
        public string BaseUrl { get; set; }
        public string TestSecretName { get; set; }
        public string Token { get; set; }
        public string VaultRole { get; set; }
        public string MountPoint { get; set; }
    }
}