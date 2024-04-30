using System;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Contract;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Contract.Exceptions;

namespace NetworkPerspective.Sync.Orchestrator.Infrastructure.Core.Impl;

internal class Core : ICore
{
    private readonly ISyncHashedClient _client;
    private readonly CoreConfig _coreConfig;
    private readonly ILogger<Core> _logger;

    public Core(ISyncHashedClient client, IOptions<CoreConfig> coreConfig, ILogger<Core> logger)
    {
        _client = client;
        _coreConfig = coreConfig.Value;
        _logger = logger;
    }

    public async Task<TokenValidationResponse> ValidateTokenAsync(SecureString accessToken, CancellationToken stoppingToken = default)
    {
        try
        {
            var result = await _client.QueryAsync(accessToken.ToSystemString(), stoppingToken);

            return new TokenValidationResponse(result.NetworkId.Value, result.ConnectorId.Value);
        }
        catch (ApiException aex) when (aex.StatusCode == (int)HttpStatusCode.Forbidden)
        {
            throw new InvalidTokenException(_coreConfig.BaseUrl);
        }
        catch (Exception ex)
        {
            throw new CoreException(_coreConfig.BaseUrl, ex);
        }
    }
}
