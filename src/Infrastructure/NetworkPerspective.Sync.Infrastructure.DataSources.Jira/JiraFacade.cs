using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Mappers;
using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Model;
using NetworkPerspective.Sync.Worker.Application.Domain.Employees;
using NetworkPerspective.Sync.Worker.Application.Domain.Sync;
using NetworkPerspective.Sync.Worker.Application.Infrastructure.DataSources;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira;

internal class JiraFacade(IHashingService hashingService, IJiraAuthorizedFacade jiraFacade) : IDataSource
{
    public async Task<EmployeeCollection> GetEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        var projectMembers = await context.EnsureSetAsync(() => GetProjectMembersAsync(stoppingToken));
        var connectorProperties = context.GetConnectorProperties<JiraConnectorProperties>();

        return EmployeesMapper.ToEmployees(projectMembers, hashingService.Hash, context.NetworkConfig.EmailFilter, connectorProperties.SyncGroupAccess);
    }

    public async Task<EmployeeCollection> GetHashedEmployeesAsync(SyncContext context, CancellationToken stoppingToken = default)
    {
        var projectMembers = await context.EnsureSetAsync(() => GetProjectMembersAsync(stoppingToken));

        return HashedEmployeesMapper.ToEmployees(projectMembers, hashingService.Hash, context.NetworkConfig.EmailFilter);
    }

    public Task<SyncResult> SyncInteractionsAsync(IInteractionsStream stream, SyncContext context, CancellationToken stoppingToken = default)
        => Task.FromResult(SyncResult.Empty);

    private async Task<List<ProjectMember>> GetProjectMembersAsync(CancellationToken stoppingToken)
    {
        var projectMembers = new List<ProjectMember>();

        var accessibleResources = await jiraFacade.GetAccessibleResourcesAsync(stoppingToken);

        foreach (var accessibleResource in accessibleResources)
        {
            var projectContainer = new ProjectContainer(accessibleResource.Id);
            var jiraProjects = await jiraFacade.GetProjectsAsync(accessibleResource.Id, stoppingToken);

            foreach (var jiraProject in jiraProjects)
            {
                var project = new Project(jiraProject.Key, jiraProject.Name, projectContainer);

                var jiraUsers = await jiraFacade.GetProjectsUsersAsync(accessibleResource.Id, jiraProject.Key, stoppingToken);
                var jiraUsersIds = jiraUsers.Select(x => x.Id);
                var jiraUsersDetails = await jiraFacade.GetUsersDetailsAsync(accessibleResource.Id, jiraUsersIds, stoppingToken);

                foreach (var jiraUserDetails in jiraUsersDetails)
                {
                    if (!projectMembers.Any(x => x.Id == jiraUserDetails.Id))
                        projectMembers.Add(new ProjectMember(jiraUserDetails.Id, jiraUserDetails.Email));

                    projectMembers.Single(x => x.Id == jiraUserDetails.Id).Projects.Add(project);
                }
            }
        }

        return projectMembers;
    }
}