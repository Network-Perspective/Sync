using System;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Application.Services;

[Obsolete($"Please use {nameof(ISecretRotationService)} instead")]
public interface ISecretRotator
{
    Task RotateSecrets();
}