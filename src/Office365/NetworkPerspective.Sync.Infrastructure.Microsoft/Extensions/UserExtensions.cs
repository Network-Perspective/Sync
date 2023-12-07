using System;
using System.Collections.Generic;

using Microsoft.Graph.Models;

using DomainGroup = NetworkPerspective.Sync.Application.Domain.Employees.Group;

namespace NetworkPerspective.Sync.Infrastructure.Microsoft.Extensions
{
    public static class UserExtensions
    {
        public static IList<DomainGroup> GetDepartmentGroups(this User user)
        {
            var groups = new List<DomainGroup>();

            if (user.Department is not null)
                groups.Add(DomainGroup.Create($"Department:{user.Department}", user.Department, DomainGroup.DepartmentCatergory));

            return groups;
        }

        public static string GetFullName(this User user)
            => user.DisplayName ?? string.Empty;
    }
}