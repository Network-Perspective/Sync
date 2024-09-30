using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira;

internal class JiraFacade(IJiraAuthorizedFacade jiraFacade) : IDataSource
{
    public async Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {

        var result = await jiraFacade.GetAccessibleResourcesAsync(stoppingToken);
        foreach (var item in result)
        {

            var projects = await jiraFacade.GetProjectsAsync(item.Id, stoppingToken);

            foreach (var project in projects)
            {
                var users = await jiraFacade.GetProjectsUsersAsync(item.Id, project.Key, stoppingToken);
                var usersDetails = await jiraFacade.GetUsersDetailsAsync(item.Id, users.Select(x => x.Id), stoppingToken);
            }
        }

        throw new NotImplementedException();
    }

    public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
        => Task.FromResult(SyncResult.Empty);
}