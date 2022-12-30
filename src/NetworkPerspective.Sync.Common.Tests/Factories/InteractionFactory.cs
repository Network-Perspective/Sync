using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;

namespace NetworkPerspective.Sync.Common.Tests.Factories
{
    public static class InteractionFactory
    {
        private static int counter = 0;

        public static Interaction Create()
        {
            counter++;

            var group1 = Group.Create(Guid.NewGuid().ToString(), $"group1Name_{counter}", $"category1_{counter}");
            var group2 = Group.Create(Guid.NewGuid().ToString(), $"group2Name_{counter}", $"category2_{counter}");

            var timestamp = DateTime.UtcNow;
            var sourceId = EmployeeId.Create($"source_{counter}", "test");
            var sourceGroups = new[] { group1, group2 };
            var source = Employee.CreateInternal(sourceId, sourceGroups);

            var targetId = EmployeeId.Create($"target_{counter}", "test");
            var targetGroups = new[] { group1 };
            var target = Employee.CreateInternal(targetId, targetGroups);

            return Interaction.CreateEmail(timestamp, source, target, $"eventid_{counter}");
        }

        public static ISet<Interaction> CreateSet(int count)
        {
            return Enumerable
                .Range(0, count)
                .Select(x => Create())
                .ToHashSet();
        }
    }
}
