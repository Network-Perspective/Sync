using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Cli
{
    public struct ColumnDescriptor
    {
        public string Header { get; set; }
        public string Name { get; set; }
        public string InRoundBrackets { get; set; }
        public string InSquareBrackets { get; set; }
    }

    public static class ParsingHelpers
    {
        public static Dictionary<string, ColumnDescriptor> AsColumnDescriptorDictionary(this string commaSeperatedCols)
        {
            if (string.IsNullOrWhiteSpace(commaSeperatedCols))
                return new Dictionary<string, ColumnDescriptor>();

            return
                commaSeperatedCols.Split(',')
                .Select(c => AsColumnDescriptor(c))
                .Where(c => c.Name != null)
                .ToDictionary(c => c.Name, c => c, StringComparer.InvariantCultureIgnoreCase);
        }

        public static ColumnDescriptor AsColumnDescriptor(this string header)
        {
            var result = new ColumnDescriptor()
            {
                Header = header
            };

            if (string.IsNullOrWhiteSpace(header))
                return result;

            result.Name = header.Trim();

            // round brackets present
            if (header.Contains('(') && header.Contains(')'))
            {
                result.InRoundBrackets =
                    header
                        .Substring(header.IndexOf('(') + 1, header.IndexOf(')') - 1 - header.IndexOf('('))
                        .Trim();
                result.Name = header.Substring(0, header.IndexOf('(')).Trim();
            }

            // square brackets present
            if (header.Contains('[') && header.Contains(']'))
            {
                result.InSquareBrackets =
                    header
                        .Substring(header.IndexOf('[') + 1, header.IndexOf(']') - 1 - header.IndexOf('['))
                        .Trim();
                result.Name = header.Substring(0, header.IndexOf('[')).Trim();
            }

            // both square and round brackets present
            if (header.Contains('(') && header.Contains(')') && header.Contains('[') && header.Contains(']'))
            {
                result.Name = header.Substring(0, Math.Min(header.IndexOf('['), header.IndexOf('('))).Trim();
            }

            return result;
        }

        public static bool IsJsonField(this ColumnDescriptor field)
        {
            return field.InSquareBrackets != null && field.InSquareBrackets.Contains("json", StringComparison.InvariantCultureIgnoreCase);
        }

        public static DateTime AsUtcDate(this string value, string? timeZoneId = null)
        {
            DateTime parsed;

            // guess the format 
            if (!DateTime.TryParseExact(value, "s", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out parsed))
            {
                parsed = DateTime.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            }
            if (timeZoneId == null)
            {
                return parsed;
            }
            
            TimeZoneInfo convFrom = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            var offset = convFrom.GetUtcOffset(parsed);
            return parsed.Add(-offset);
        }
    }
}