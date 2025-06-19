using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Utils.Extensions;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Extensions;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface ISyncService
{
    Task<SyncResult> SyncAsync(SyncContext syncContext, CancellationToken stoppingToken = default);
}

internal sealed class SyncService(ILogger<SyncService> logger,
                   IDataSource dataSource,
                   INetworkPerspectiveCore networkPerspectiveCore,
                   IStatusLogger statusLogger,
                   IInteractionsFilterFactory interactionFilterFactory,
                   IConnectorTypesCollection connectorTypes) : ISyncService
{
    public async Task<SyncResult> SyncAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        try
        {
            await dataSource.ValidateAsync(context, stoppingToken);
            var connectorProperties = new ConnectorProperties(context.ConnectorProperties);
            await statusLogger.LogInfoAsync("Sync started", stoppingToken);
            logger.LogInformation(
                "Executing synchronization for Connector '{connectorId}' for timerange '{timeRange}'",
                context.ConnectorId, context.TimeRange);

            await networkPerspectiveCore.ReportSyncStartAsync(context.AccessToken, context.TimeRange, stoppingToken);

            await SyncGroupsAsync(dataSource, context, stoppingToken);
            await SyncUsersAsync(dataSource, context, stoppingToken);
            await SyncEntitiesAsync(dataSource, context, stoppingToken);

            var syncResult = await SyncInteractionsAsync(dataSource, context, stoppingToken);

            var message = $"Sync completed successfully. Success rate: {syncResult.SuccessRate}. TasksCount: {syncResult.TasksCount}. FailedTasksCount: {syncResult.FailedTasksCount}. TotalInteractionsCount: {syncResult.TotalInteractionsCount}";

            var errors = syncResult.Exceptions?
                .Select(e => $"{e.GetType().FullName}:{e.Message}:{e.StackTrace}")
                .GroupBy(e => e)
                .Select(e => $"Exception | Count: {e.Count()} | Stacktrace: {e.Key} \n")
                .ToList();

            if (errors != null && errors.Any())
            {
                message += string.Join("\n", errors);
            }

            await networkPerspectiveCore.ReportSyncSuccessfulAsync(context.AccessToken, context.TimeRange, message, stoppingToken);
            await statusLogger.LogInfoAsync("Sync completed", stoppingToken);
            logger.LogInformation("Synchronization completed for Connector '{connectorId}'", context.ConnectorId);

            return syncResult;
        }
        catch (ValidationException ve) // TODO maybe remove after excel validation introduced
        {
            await networkPerspectiveCore.TryReportSyncFailedAsync(context.AccessToken, context.TimeRange, ve.Message, stoppingToken);
            logger.LogError(ve, "Cannot complete synchronization job for Connector {connectorId}.\n{exceptionMessage}", context.ConnectorId, ve.Message);
            await statusLogger.LogErrorAsync($"Sync cancelled because of validation error '{ve.Message}'", CancellationToken.None);
            return SyncResult.Empty;
        }
        catch (FluentValidation.ValidationException ve)
        {
            await networkPerspectiveCore.TryReportSyncFailedAsync(context.AccessToken, context.TimeRange, ve.Message, stoppingToken);
            logger.LogError(ve, "Cannot complete synchronization job for Connector {connectorId} because validation exception has been thrown", context.ConnectorId);
            await statusLogger.LogErrorAsync($"Sync failed because of validation error '{ve.Message}'", CancellationToken.None);
            return SyncResult.Empty;
        }
        catch (Exception ex) when (ex.IndicatesTaskCanceled())
        {
            await networkPerspectiveCore.TryReportSyncFailedAsync(context.AccessToken, context.TimeRange, ex.Message, stoppingToken);
            logger.LogInformation("Synchronization Job cancelled for Connector '{connectorId}'", context.ConnectorId);
            await statusLogger.LogInfoAsync("Sync cancelled", CancellationToken.None);
            return SyncResult.Empty;
        }
        catch (Exception ex)
        {
            await networkPerspectiveCore.TryReportSyncFailedAsync(context.AccessToken, context.TimeRange, ex.Message, stoppingToken);
            logger.LogError(ex, "Cannot complete synchronization job for Connector {connectorId}.\n{exceptionMessage}", context.ConnectorId, ex.Message);
            await statusLogger.LogErrorAsync("Sync failed", CancellationToken.None);
            return SyncResult.Empty;
        }
    }

    private async Task SyncUsersAsync(IDataSource dataSource, SyncContext context, CancellationToken stoppingToken)
    {
        if (!new ConnectorProperties(context.ConnectorProperties).SyncEmployees)
        {
            await statusLogger.LogInfoAsync("Skipping sync employees profiles", stoppingToken);
            return;
        }

        logger.LogInformation("Synchronizing employees profiles for connector '{connectorId}'", context.ConnectorId);
        await statusLogger.LogInfoAsync($"Synchronizing employees profiles...", stoppingToken);

        var employees = await dataSource.GetEmployeesAsync(context, stoppingToken);
        await statusLogger.LogInfoAsync($"Received employees profiles", stoppingToken);
        var dataSourceIdName = connectorTypes[context.ConnectorType].DataSourceId;
        await networkPerspectiveCore.PushUsersAsync(context.AccessToken, employees, dataSourceIdName, stoppingToken);
        await statusLogger.LogInfoAsync($"Uploaded employees profiles", stoppingToken);

        await statusLogger.LogInfoAsync($"Synchronization of employees profiles completed", stoppingToken);
        logger.LogInformation("Synchronization of employees profiles for connector '{connectorId}' completed", context.ConnectorId);
    }

    private async Task SyncEntitiesAsync(IDataSource dataSource, SyncContext context, CancellationToken stoppingToken)
    {
        if (!new ConnectorProperties(context.ConnectorProperties).SyncHashedEmployees)
        {
            await statusLogger.LogInfoAsync("Skipping sync hashed employees profiles", stoppingToken);
            return;
        }

        logger.LogInformation("Synchronizing hashed employees profiles for connector '{connectorId}'", context.ConnectorId);
        await statusLogger.LogInfoAsync($"Synchronizing hashed employees profiles...", stoppingToken);

        var employees = await dataSource.GetHashedEmployeesAsync(context, stoppingToken);
        await statusLogger.LogInfoAsync($"Received hashed employees profiles", stoppingToken);
        var dataSourceId = connectorTypes[context.ConnectorType].DataSourceId;
        await networkPerspectiveCore.PushEntitiesAsync(context.AccessToken, employees, context.TimeRange.Start, dataSourceId, stoppingToken);
        await statusLogger.LogInfoAsync($"Uploaded hashed employees profiles", stoppingToken);

        await statusLogger.LogInfoAsync($"Synchronization of hashed employees profiles completed", stoppingToken);
        logger.LogInformation("Synchronization of hashed employees profiles for connector '{connectorId}' completed", context.ConnectorId);
    }

    private async Task SyncGroupsAsync(IDataSource dataSource, SyncContext context, CancellationToken stoppingToken)
    {
        if (!new ConnectorProperties(context.ConnectorProperties).SyncGroups)
        {
            await statusLogger.LogInfoAsync("Skipping sync groups", stoppingToken);
            return;
        }

        logger.LogInformation("Synchronizing groups for connector '{connectorId}'", context.ConnectorId);
        await statusLogger.LogInfoAsync($"Synchronizing groups...", stoppingToken);

        var employees = await dataSource.GetHashedEmployeesAsync(context, stoppingToken);

        var groups = employees
            .GetAllInternal()
            .SelectMany(x => x.Groups)          // flatten groups
            .ToHashSet(Group.EqualityComparer); // only distinct values

        if (!new ConnectorProperties(context.ConnectorProperties).SyncChannelsNames)
        {
            groups = groups
                .Where(x => x.Category != Group.ChannelCategory)
                .ToHashSet(Group.EqualityComparer);
        }

        await statusLogger.LogInfoAsync($"Received {groups.Count} groups", stoppingToken);

        await networkPerspectiveCore.PushGroupsAsync(context.AccessToken, groups, stoppingToken);
        await statusLogger.LogInfoAsync("Uploaded groups", stoppingToken);

        await statusLogger.LogInfoAsync($"Synchronization of groups completed", stoppingToken);
        logger.LogInformation("Synchronization of groups for connector '{connectorId}' completed", context.ConnectorId);
    }

    private async Task<SyncResult> SyncInteractionsAsync(IDataSource dataSource, SyncContext context, CancellationToken stoppingToken)
    {
        if (!new ConnectorProperties(context.ConnectorProperties).SyncInteractions)
        {
            await statusLogger.LogInfoAsync("Skipping sync interactions", stoppingToken);
            return SyncResult.Empty;
        }

        logger.LogInformation("Synchronizing interactions for connector '{connectorId}' for period {period}", context.ConnectorId, context.TimeRange);
        await statusLogger.LogInfoAsync($"Synchronizing interactions for period '{context.TimeRange}'...", stoppingToken);

        var filter = interactionFilterFactory
            .CreateInteractionsFilter(context.TimeRange);

        var stream = networkPerspectiveCore.OpenInteractionsStream(context.AccessToken, connectorTypes[context.ConnectorType].DataSourceId, stoppingToken);
        await using var filteredStream = new FilteredInteractionStreamDecorator(stream, filter);

        var result = await dataSource.SyncInteractionsAsync(filteredStream, context, stoppingToken);

        await statusLogger.LogInfoAsync($"Synchronization of interactions for period '{context.TimeRange}' completed", stoppingToken);
        logger.LogInformation("Synchronization of interactions for connector '{connectorId}' for {period} completed{newLine}{result}", context.ConnectorId, context.TimeRange, Environment.NewLine, result);

        foreach (var ex in result.Exceptions)
            logger.LogWarning(ex, "Task thrown exception");

        return result;
    }
}