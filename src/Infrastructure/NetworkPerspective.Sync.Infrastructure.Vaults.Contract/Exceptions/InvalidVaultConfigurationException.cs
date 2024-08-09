namespace NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions;

public class InvalidVaultConfigurationException : VaultException
{
    public InvalidVaultConfigurationException(string message) : base(message)
    {
    }
}