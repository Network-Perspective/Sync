using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
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

        [Theory]
        // some random points in time
        [InlineData("2022-10-20T10:00:00", "Central European Standard Time", "Europe/Warsaw", "2022-10-20T08:00:00")]  // UTC+2
        [InlineData("2022-12-20T10:00:00", "Central European Standard Time", "Europe/Warsaw", "2022-12-20T09:00:00")]  // UTC+1 
        // Zmiana czasu na zimowy odbywa się zawsze w ostatnią niedzielę października
        [InlineData("2022-10-29T10:00:00", "Central European Standard Time", "Europe/Warsaw", "2022-10-29T08:00:00")]  // UTC+2
        [InlineData("2022-10-30T10:00:00", "Central European Standard Time", "Europe/Warsaw", "2022-10-30T09:00:00")]  // UTC+1
        // Zmiana czasu na letni odbywa się zawsze w ostatnią niedzielę marca
        [InlineData("2022-03-26T10:00:00", "Central European Standard Time", "Europe/Warsaw", "2022-03-26T09:00:00")]  // UTC+1
        [InlineData("2022-03-27T10:00:00", "Central European Standard Time", "Europe/Warsaw", "2022-03-27T08:00:00")]  // UTC+2
        // no time zone => assume utc
        [InlineData("2022-03-26T10:00:00", null, null, "2022-03-26T10:00:00")]
        [InlineData("2022-03-26T10:00:00", "UTC", "UTC", "2022-03-26T10:00:00")]
        // just date in utc
        [InlineData("2022-03-26", null, null, "2022-03-26T00:00:00")]
        public void ItShouldShiftTimeZone(string localTime, string timeZoneWindows, string timeZoneLinux, string utc)
        {
            // this is system time zone id so it differes in windows and linux
            string timeZone = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? timeZoneWindows : timeZoneLinux;
            var expected = DateTime.ParseExact(utc, "s", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);

            // act
            var actual = ParsingHelpers.AsUtcDate(localTime, timeZone);

            // assert
            actual.Should().Be(expected);
        }
    }
}