using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Cli.Tests
{
    public class ParsingHelpersTest
    {
        [Theory]
        [InlineData("Name", "Name")]
        [InlineData("When", "When")]
        public void ItShouldParseBasicField(string header, string fieldName)
        {
            var actual = ParsingHelpers.AsColumnDescriptor(header);

            actual.Name.Should().Be(fieldName);
            actual.InRoundBrackets.Should().BeNull();
            actual.InSquareBrackets.Should().BeNull();
        }


        [Theory]
        [InlineData("From (EmployeeId)", "From", "EmployeeId")]
        [InlineData("From(EmployeeId)", "From", "EmployeeId")]
        [InlineData("From ( EmployeeId ) ", "From", "EmployeeId")]
        [InlineData(" From (EmployeeId) ", "From", "EmployeeId")]
        public void ItShouldParseRoundBrackets(string header, string fieldName, string inRoundBrackets)
        {
            var actual = ParsingHelpers.AsColumnDescriptor(header);

            actual.Name.Should().Be(fieldName);
            actual.InRoundBrackets.Should().Be(inRoundBrackets);
            actual.InSquareBrackets.Should().BeNull();
        }


        [Theory]
        [InlineData("ArrayField [Json]", "ArrayField", "Json")]
        [InlineData("ArrayField[Json]", "ArrayField", "Json")]
        [InlineData("ArrayField [ Json ] ", "ArrayField", "Json")]
        [InlineData(" ArrayField [Json] ", "ArrayField", "Json")]
        public void ItShouldParseSquareBrackets(string header, string fieldName, string inSquareBrackets)
        {
            var actual = ParsingHelpers.AsColumnDescriptor(header);

            actual.Name.Should().Be(fieldName);
            actual.InSquareBrackets.Should().Be(inSquareBrackets);
            actual.InRoundBrackets.Should().BeNull();
        }


        [Theory]
        [InlineData("Suboordinates [Json] (EmployeeId)", "Suboordinates", "Json", "EmployeeId")]
        [InlineData("Suboordinates[Json](EmployeeId)", "Suboordinates", "Json", "EmployeeId")]
        [InlineData("Suboordinates [ Json ] ( EmployeeId )", "Suboordinates", "Json", "EmployeeId")]
        [InlineData(" Suboordinates [Json]  (EmployeeId) ", "Suboordinates", "Json", "EmployeeId")]
        [InlineData("Suboordinates (EmployeeId) [Json] ", "Suboordinates", "Json", "EmployeeId")]
        [InlineData("Suboordinates(EmployeeId)[Json]", "Suboordinates", "Json", "EmployeeId")]
        [InlineData("Suboordinates ( EmployeeId ) [ Json ]", "Suboordinates", "Json", "EmployeeId")]
        [InlineData(" Suboordinates (EmployeeId)  [Json]   ", "Suboordinates", "Json", "EmployeeId")]
        public void ItShouldParseSquareAndroundBrackets(string header, string fieldName, string inSquareBrackets, string inRoundBrackets)
        {
            var actual = ParsingHelpers.AsColumnDescriptor(header);

            actual.Name.Should().Be(fieldName);
            actual.InSquareBrackets.Should().Be(inSquareBrackets);
            actual.InRoundBrackets.Should().Be(inRoundBrackets);
        }

        [Fact]
        public void ItShouldMultipleColscommaSeperated()
        {
            var actual = ParsingHelpers.AsColumnDescriptorDictionary("Id,Code(EmployeeId), Other [Json] ");

            actual["Id"].Name.Should().Be("Id");
            actual["Id"].InRoundBrackets.Should().BeNull();
            actual["Id"].InSquareBrackets.Should().BeNull();

            actual["Code"].Name.Should().Be("Code");
            actual["Code"].InRoundBrackets.Should().Be("EmployeeId");
            actual["Code"].InSquareBrackets.Should().BeNull();

            actual["Other"].Name.Should().Be("Other");
            actual["Other"].InRoundBrackets.Should().BeNull();
            actual["Other"].InSquareBrackets.Should().Be("Json");
        }
    }
}