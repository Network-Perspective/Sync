using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using NetworkPerspective.Sync.Application.Domain.Aggregation;

using Xunit;

namespace NetworkPerspective.Sync.Application.Tests.Domain.Aggregation
{
    public class DefaultActionsAggregatorPrinterTests
    {
        [Fact]
        public void ShouldPrint()
        {
            // Arrange
            const string subject = "foo";
            var date1 = new DateTime(2022, 01, 01);
            var date2 = new DateTime(2022, 01, 01);

            var actionsAggregator = new ActionsAggregator(subject);
            actionsAggregator.Add(date1);
            actionsAggregator.Add(date2);
            actionsAggregator.Add(date2);

            var printer = new DefaultActionsAggregatorPrinter();

            // Act
            var result = printer.Print(actionsAggregator);

            // Assert
            result.Should().Contain(subject);
            result.Should().Contain(date1.ToString(DefaultActionsAggregatorPrinter.DateTimeFormat));
            result.Should().Contain(date2.ToString(DefaultActionsAggregatorPrinter.DateTimeFormat));
        }

        [Fact]
        public void ShouldPrintEmptyData()
        {
            // Arrange
            const string subject = "foo";

            var actionsAggregator = new ActionsAggregator(subject);

            var printer = new DefaultActionsAggregatorPrinter();

            // Act
            var result = printer.Print(actionsAggregator);

            // Assert
            result.Should().Contain(subject);
            result.Should().Contain("has no actions");
        }
    }
}