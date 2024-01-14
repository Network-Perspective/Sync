using System;
using System.Collections.Generic;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Networks.Filters;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Networks.Filters
{
    public class EmployeeFilterTests
    {
        public class IsInternal : EmployeeFilterTests
        {
            [Theory]
            [ClassData(typeof(IsInternal.PositiveResultTestData))]
            public void ShouldReturnTrue(IEnumerable<string> whitelist, IEnumerable<string> blacklist, string email, string group)
            {
                // Arrange
                var filter = new EmployeeFilter(whitelist, blacklist);
            
                // Act
                var result = filter.IsInternal(email, group);

                // Assert
                result.Should().BeTrue(because: "the filter should return true on blacklist not containing the email and whitelist allows (using email or group)");
            }

            [Theory]
            [ClassData(typeof(IsInternal.NegativeResultTestData))]
            public void ShouldReturnFalse(IEnumerable<string> whitelist, IEnumerable<string> blacklist, string email, string group)
            {
                // Arrange
                var filter = new EmployeeFilter(whitelist, blacklist);

                // Act
                var result = filter.IsInternal(email, group);

                // Assert
                result.Should().BeFalse(because: "the filter should return false on blacklist containing the email or whitelist not allows (using email or group)");
            }

            internal class TestData : TheoryData<IEnumerable<string>, IEnumerable<string>, string, string>
            {
                protected void AddTyped(IEnumerable<string> whitelist, IEnumerable<string> blacklist, string email, string group)
                    => Add(whitelist, blacklist, email, group);
            }

            internal class PositiveResultTestData : TestData
            {
                public PositiveResultTestData()
                {
                    AddTyped(
                        whitelist: new[] { "john.doe@networkperspective.io" },
                        blacklist: Array.Empty<string>(),
                        email: "john.doe@networkperspective.io",
                        group: string.Empty);

                    AddTyped(
                        whitelist: new[] { "email:*@networkperspective.io" },
                        blacklist: Array.Empty<string>(),
                        email: "john.doe@networkperspective.io",
                        group: string.Empty);

                    AddTyped(
                        whitelist: new[] { "group:networkperspective" },
                        blacklist: Array.Empty<string>(),
                        email: "john.doe@networkperspective.io",
                        group: "networkperspective");

                    AddTyped(
                        whitelist: Array.Empty<string>(),
                        blacklist: Array.Empty<string>(),
                        email: "john.doe@networkperspective.io",
                        group: "networkperspective");

                    AddTyped(
                        whitelist: new[] { "email: *@networkperspective.io", "group: networkperspective"},
                        blacklist: Array.Empty<string>(),
                        email: "john.doe@networkperspective.io",
                        group: "networkperspective");

                    AddTyped(
                        whitelist: null,
                        blacklist: Array.Empty<string>(),
                        email: "john.doe@networkperspective.io",
                        group: null);
                }
            }

            internal class NegativeResultTestData : TestData
            {
                public NegativeResultTestData()
                {
                    AddTyped(
                        whitelist: new[] { "*@networkperspective.io" },
                        blacklist: new[] { "john.doe@networkperspective.io" },
                        email: "john.doe@networkperspective.io",
                        group: string.Empty);

                    AddTyped(
                        whitelist: new[] { "*@non-existing-domain" },
                        blacklist: Array.Empty<string>(),
                        email: "john.doe@networkperspective.io",
                        group: string.Empty);

                    AddTyped(
                        whitelist: new[] { "group: networkperspective.io" },
                        blacklist: Array.Empty<string>(),
                        email: "john.doe@networkperspective.io",
                        group: "other-group");
                }
            }
        }
    }
}