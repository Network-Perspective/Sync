using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Services;

public interface ISecretRotator
{
    Task RotateSecrets();
}