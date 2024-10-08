﻿using System;
using System.Collections.Generic;

using FluentAssertions;

using Microsoft.Graph.Models;

using NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Mappers;
using NetworkPerspective.Sync.Worker.Application.Domain;
using NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters;

using Xunit;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Tests.Mappers
{
    public class EmployeesMapperTests
    {
        [Fact]
        public void ShouldMapToEmployeesCollection()
        {
            // Arrange
            var users = new[]
            {
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Mail = "john@networkperspective.io",
                    OtherMails = new List<string> { "john@networkperspective.com"}
                },
                new User
                {
                    Id = Guid.NewGuid().ToString(),
                    Mail = "alice@networkperspective.io",
                    OtherMails = new List<string> { "alice@networkperspective.com"}
                }
            };

            // Act
            var employees = EmployeesMapper.ToEmployees(users, HashFunction.Empty, EmployeeFilter.Empty, false);

            // Assert
            employees.GetAllInternal().Should().NotBeEmpty();
        }
    }
}