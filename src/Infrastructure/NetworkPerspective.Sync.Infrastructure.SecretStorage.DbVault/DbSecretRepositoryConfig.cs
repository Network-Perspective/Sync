namespace NetworkPerspective.Sync.Infrastructure.SecretStorage.DbVault
{
    public class DbSecretRepositoryConfig
    {
        public string PublicKeyPath { get; set; }
        public string PrivateKeyPath { get; set; }
        public string SecretsPath { get; set; }
    }
}