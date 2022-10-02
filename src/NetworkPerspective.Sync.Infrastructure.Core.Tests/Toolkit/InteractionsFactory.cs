using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Application.Domain.Employees;
using NetworkPerspective.Sync.Application.Domain.Interactions;

namespace NetworkPerspective.Sync.Infrastructure.Core.Tests.Toolkit
{
    internal static class InteractionsFactory
    {
        private static int _counter = 0;

        public static ISet<Interaction> Create(int count)
        {
            var result = new HashSet<Interaction>(new InteractionEqualityComparer());

            for (int i = 0; i < count; i++)
                result.Add(Create());

            return result;
        }

        public static Interaction Create()
        {
            var group1 = Group.Create(Guid.NewGuid().ToString(), $"group1Name_{_counter}", $"category1_{_counter}");
            var group2 = Group.Create(Guid.NewGuid().ToString(), $"group2Name_{_counter}", $"category2_{_counter}");

            var source = Employee.CreateInternal($"email_source_{_counter}@networkperspective.io", $"email_source_{_counter}_manager@networkperspective.io", new[] { group1, group2 });
            var target = Employee.CreateInternal($"email_target_{_counter}@networkperspective.io", $"email_target_{_counter}_manager@networkperspective.io", new[] { group2 });

            _counter++;

            return Interaction.CreateEmail(DateTime.UtcNow, source, target, Guid.NewGuid().ToString());
        }
    }
}