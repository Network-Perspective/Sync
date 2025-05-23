﻿using System;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Utils.CQS.Middlewares;
using NetworkPerspective.Sync.Utils.CQS.Queries;
using NetworkPerspective.Sync.Worker.Application.Exceptions;
using NetworkPerspective.Sync.Worker.Application.Extensions;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Worker.Application.UseCases.Middlewares;

internal class ConnectorScopedLoggingMiddleware(IStatusLogger statusLogger) : IMediatorMiddleware
{
    async Task<TResponse> IMediatorMiddleware.HandleQueryAsync<TRequest, TResponse>(TRequest request, QueryHandlerDelegate<TRequest, TResponse> next, CancellationToken cancellationToken)
    {
        if (request is ConnectorStatusRequest)
            return await next(request, cancellationToken);

        try
        {
            await statusLogger.LogDebugAsync($"Running action '{request.UserFriendlyName}'", cancellationToken);
            var result = await next(request, cancellationToken);
            await statusLogger.LogDebugAsync($"Action completed '{request.UserFriendlyName}'", cancellationToken);
            return result;
        }
        catch (SyncAlreadyInProgressException)
        {
            await statusLogger.LogDebugAsync($"Unable to perform action '{request.UserFriendlyName}'. Synchronization is already in progress", cancellationToken);
            throw;
        }
        catch (Exception)
        {
            await statusLogger.LogWarningAsync($"Unable to perform action '{request.UserFriendlyName}'", cancellationToken);
            throw;
        }
    }
}