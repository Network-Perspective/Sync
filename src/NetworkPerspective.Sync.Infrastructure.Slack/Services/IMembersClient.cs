using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Networks;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
    internal interface IMembersClient
    {
        Task<EmployeeCollection> GetEmployees(ISlackClientFacade slackClientFacade, EmailFilter emailFilter, CancellationToken stoppingToken = default);
        Task<EmployeeCollection> GetHashedEmployees(ISlackClientFacade slackClientFacade, EmailFilter emailFilter, HashFunction hashFunc, CancellationToken stoppingToken = default);
    }

    internal class MembersClient : IMembersClient
    {
        private const string SlackBotId = "USLACKBOT";

        private readonly ILogger<MembersClient> _logger;

        public MembersClient(ILogger<MembersClient> logger)
        {
            _logger = logger;
        }

        public Task<EmployeeCollection> GetEmployees(ISlackClientFacade slackClientFacade, EmailFilter emailFilter, CancellationToken stoppingToken = default)
            => GetEmployeesInternalAsync(slackClientFacade, emailFilter, null, stoppingToken);

        public Task<EmployeeCollection> GetHashedEmployees(ISlackClientFacade slackClientFacade, EmailFilter emailFilter, HashFunction hashFunc, CancellationToken stoppingToken = default)
            => GetEmployeesInternalAsync(slackClientFacade, emailFilter, hashFunc, stoppingToken);

        private async Task<EmployeeCollection> GetEmployeesInternalAsync(ISlackClientFacade slackClientFacade, EmailFilter emailFilter, HashFunction hashFunc, CancellationToken stoppingToken = default)
        {
            if (hashFunc == null)
                _logger.LogDebug("Fetching employees... Skipping hashing due to null {func}", nameof(hashFunc));
            else
                _logger.LogDebug("Fetching employees...");

            var allSlackUsers = await slackClientFacade.GetAllUsers(stoppingToken);

            var slackUsers = allSlackUsers
                .Where(x => emailFilter.IsInternalUser(x.Profile.Email))
                .Where(x => x.IsBot == false)
                .Where(x => x.Id != SlackBotId); // please see https://stackoverflow.com/questions/40679819

            var botsIds = allSlackUsers
                .Where(x => x.IsBot == true || x.Id == SlackBotId)
                .Select(x => x.Id)
                .ToHashSet(StringComparer.InvariantCultureIgnoreCase);

            if (!slackUsers.Any())
                _logger.LogWarning("No employees found");
            else
                _logger.LogDebug("Fetching organization members completed (employees: '{employeesCount}', bots: '{botsCount}')", slackUsers.Count(), botsIds.Count);

            var employees = new List<Employee>();

            foreach (var slackUser in slackUsers)
            {
                var usersChannels = await slackClientFacade.GetAllUsersChannels(slackUser.Id, stoppingToken);
                var groups = usersChannels.Select(x => Group.Create(x.Id, x.Name, "Project"));
                var employeeId = EmployeeId.Create(slackUser.Profile.Email, slackUser.Id);
                var employee = Employee.CreateInternal(employeeId, groups);
                employees.Add(employee);
                _logger.LogTrace("User: '{email}'", slackUser.Profile.Email);
            }

            foreach (var botId in botsIds)
            {
                var bot = Employee.CreateBot(botId);
                employees.Add(bot);
            }

            return new EmployeeCollection(employees, hashFunc);
        }
    }
}