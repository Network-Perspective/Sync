using System.Security;

using NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Contract;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Stub;

internal class CoreStub : ICore
{
    private static readonly Guid NetworkId = new("bd1bc916-db78-4e1e-b93b-c6feb8cf729e");
    private static readonly Guid ConnectorId = new("04C753D8-FF9A-479C-B857-5D28C1EAF6C1");

    public Task<TokenValidationResponse> ValidateTokenAsync(SecureString accessToken, CancellationToken stoppingToken = default)
        => Task.FromResult(new TokenValidationResponse(NetworkId, ConnectorId));
}