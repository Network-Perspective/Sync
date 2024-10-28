﻿using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Worker.HostedServices;

internal class StartupSecretsChecker(IVault vault, ILogger<StartupSecretsChecker> logger) : IHostedService
{
    private readonly string[] _expectedSecretsKeys = [
        ConnectionHost.OrchestratorClientNameKey,
        ConnectionHost.OrchestratorClientNameKey
    ];

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var expectedSecretKey in _expectedSecretsKeys)
        {
            try
            {
                var secret = await vault.GetSecretAsync(expectedSecretKey, cancellationToken);

                if (string.IsNullOrEmpty(secret.ToSystemString()))
                    logger.LogError("Secret '{key}' is empty. Please make sure the secret is initialzied in key vault", expectedSecretKey);
                else
                    logger.LogInformation("Secret '{key}' initialized", expectedSecretKey);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Secret '{key}' is not set or vault is not reachable. Please see inner exception", expectedSecretKey);
            }
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
        => Task.CompletedTask;
}