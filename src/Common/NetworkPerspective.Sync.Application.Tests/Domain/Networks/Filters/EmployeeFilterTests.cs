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
            public void ShouldReturnTrue(IEnumerable<string> whitelist, IEnumerable<string> blacklist, IEnumerable<string> emails, IEnumerable<string> groups)
            {
                // Arrange
                var filter = new EmployeeFilter(whitelist, blacklist);

                // Act
                var result = filter.IsInternal(emails, groups);

                // Assert
                result.Should().BeTrue(because: "the filter should return true on blacklist not containing the email and whitelist allows (using email or group)");
            }

            [Theory]
            [ClassData(typeof(IsInternal.NegativeResultTestData))]
            public void ShouldReturnFalse(IEnumerable<string> whitelist, IEnumerable<string> blacklist, IEnumerable<string> emails, IEnumerable<string> groups)
            {
                // Arrange
                var filter = new EmployeeFilter(whitelist, blacklist);

                // Act
                var result = filter.IsInternal(emails, groups);

                // Assert
                result.Should().BeFalse(because: "the filter should return false on blacklist containing the email or whitelist not allows (using email or group)");
            }

            internal class TestData : TheoryData<IEnumerable<string>, IEnumerable<string>, IEnumerable<string>, IEnumerable<string>>
            {
                protected void AddTyped(IEnumerable<string> whitelist, IEnumerable<string> blacklist, IEnumerable<string> emails, IEnumerable<string> groups)
                    => Add(whitelist, blacklist, emails, groups);
            }

            internal class PositiveResultTestData : TestData
            {
                public PositiveResultTestData()
                {
                    AddTyped(
                        whitelist: new[] { "john.doe@networkperspective.io" },
                        blacklist: Array.Empty<string>(),
                        emails: new[] { "john.doe@networkperspective.io", "john.doe@networkperspective.com" },
                        groups: new[] { string.Empty });

                    AddTyped(
                        whitelist: new[] { "email:*@networkperspective.io" },
                        blacklist: Array.Empty<string>(),
                        emails: new[] { "john.doe@networkperspective.io", "john.doe@networkperspective.com", },
                        groups: new[] { string.Empty });

                    AddTyped(
                        whitelist: new[] { "group:networkperspective" },
                        blacklist: Array.Empty<string>(),
                        emails: new[] { "john.doe@networkperspective.io", "john.doe@networkperspective.com" },
                        groups: new[] { "networkperspective" });

                    AddTyped(
                        whitelist: Array.Empty<string>(),
                        blacklist: Array.Empty<string>(),
                        emails: new[] { "john.doe@networkperspective.io", "john.doe@networkperspective.com" },
                        groups: new[] { "networkperspective" });

                    AddTyped(
                        whitelist: new[] { "email: *@networkperspective.io", "group: networkperspective" },
                        blacklist: Array.Empty<string>(),
                        emails: new[] { "john.doe@networkperspective.io", "john.doe@networkperspective.com" },
                        groups: new[] { "networkperspective" });

                    AddTyped(
                        whitelist: null,
                        blacklist: Array.Empty<string>(),
                        emails: new[] { "john.doe@networkperspective.io", "john.doe@networkperspective.com" },
                        groups: null);

                    AddTyped(
                        whitelist: new[] { "john.doe@networkperspective.io" },
                        blacklist: Array.Empty<string>(),
                        emails: new[] { "john.doe@networkperspective.io", "john.doe@networkperspective.com" },
                        groups: new[] { (string)null });
                }
            }

            internal class NegativeResultTestData : TestData
            {
                public NegativeResultTestData()
                {
                    AddTyped(
                        whitelist: new[] { "*@networkperspective.io" },
                        blacklist: new[] { "john.doe@networkperspective.io" },
                        emails: new[] { "john.doe@networkperspective.io", "john.doe@networkperspective.com" },
                        groups: new[] { string.Empty });

                    AddTyped(
                        whitelist: new[] { "*@non-existing-domain" },
                        blacklist: Array.Empty<string>(),
                        emails: new[] { "john.doe@networkperspective.io" },
                        groups: new[] { string.Empty });

                    AddTyped(
                        whitelist: new[] { "group: networkperspective.io" },
                        blacklist: Array.Empty<string>(),
                        emails: new[] { "john.doe@networkperspective.io" },
                        groups: new[] { "other-group" });

                    AddTyped(
                        whitelist: new[] { "john.doe@networkperspective.io" },
                        blacklist: Array.Empty<string>(),
                        emails: new[] { (string)null },
                        groups: Array.Empty<string>());
                }
            }
        }
    }
}