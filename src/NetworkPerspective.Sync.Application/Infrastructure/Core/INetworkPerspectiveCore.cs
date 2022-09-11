using System.Collections.Generic;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;
using NetworkPerspective.Sync.Application.Domain.Networks;

namespace NetworkPerspective.Sync.Application.Infrastructure.Core
{
    public interface INetworkPerspectiveCore
    {
        Task PushInteractionsAsync(SecureString accessToken, ISet<Interaction> interactions, CancellationToken stoppingToken = default);
        Task PushUsersAsync(SecureString accessToken, EmployeeCollection employees, CancellationToken stoppingToken = default);
        Task PushEntitiesAsync(SecureString accessToken, EmployeeCollection employees, CancellationToken stoppingToken = default);
        Task PushGroupsAsync(SecureString accessToken, IEnumerable<Group> groups, CancellationToken stoppingToken = default);
        Task<NetworkConfig> GetNetworkConfigAsync(SecureString accessToken, CancellationToken stoppingToken = default);
        Task<TokenValidationResponse> ValidateTokenAsync(SecureString accessToken, CancellationToken stoppingToken = default);
    }
}