﻿using System;
using System.Collections.Generic;

using FluentAssertions;

using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Mappers;
using NetworkPerspective.Sync.Worker.Application.Domain;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;

using Xunit;

using DomainChannel = NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models.Channel;
using DomainTeam = NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Models.Team;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Mappers
{
    public class HashedEmployeesMapperTests
    {
        [Fact]
        public void ShouldMapToEmployeesCollection()
        {
            // Arrange
            var userId1 = Guid.NewGuid().ToString();
            var userId2 = Guid.NewGuid().ToString();

            var channels = new[]
            {
                new DomainChannel("ChannelId1", "ChannelName1", new DomainTeam("TeamId1", "TeamName1"), [userId1, userId2] ),
                new DomainChannel("ChannelId2", "ChannelName2", new DomainTeam("TeamId1", "TeamName1"), [userId1, Guid.NewGuid().ToString()] ),
            };

            var users = new[]
            {
                new User
                {
                    Id = userId1,
                    Mail = "john@networkperspective.io",
                    OtherMails = new List<string> { "john@networkperspective.com"}
                },
                new User
                {
                    Id = userId2,
                    Mail = "alice@networkperspective.io",
                    OtherMails = new List<string> { "alice@networkperspective.com"}
                }
            };

            // Act
            var employees = HashedEmployeesMapper.ToEmployees(users, channels, HashFunction.Empty, EmployeeFilter.Empty);

            // Assert
            employees.GetAllInternal().Should().NotBeEmpty();
        }
    }
}