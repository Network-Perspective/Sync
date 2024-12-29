using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Utils.CQS.Queries;
using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;
using NetworkPerspective.Sync.Worker.Application.Services;
using NetworkPerspective.Sync.Worker.Application.Services.TasksStatuses;

namespace NetworkPerspective.Sync.Worker.Application.UseCases.Handlers;

internal class ConnectorStatusHandler(IAuthTester authTester, IGlobalStatusCache tasksStatusesCache, ILogger<ConnectorStatusHandler> logger) : IRequestHandler<ConnectorStatusRequest, ConnectorStatusResponse>
{
    public async Task<ConnectorStatusResponse> HandleAsync(ConnectorStatusRequest request, CancellationToken stoppingToken = default)
    {
        logger.LogInformation("Checking connector '{connectorId}' status", request.Connector.Id);

        var isAuthorized = await authTester.IsAuthorizedAsync(stoppingToken);
        var taskStatus = await tasksStatusesCache.GetStatusAsync(request.Connector.Id, stoppingToken);

        var isRunning = taskStatus != SingleTaskStatus.Empty;

        logger.LogInformation("Status check for connector '{connectorId}' completed", request.Connector.Id);

        return new ConnectorStatusResponse
        {
            CorrelationId = request.Connector.Id,
            IsAuthorized = isAuthorized,
            IsRunning = isRunning,
            CurrentTaskCaption = taskStatus.Caption,
            CurrentTaskDescription = taskStatus.Description,
            CurrentTaskCompletionRate = taskStatus.CompletionRate
        };
    }
}