using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Application.Exceptions;

using Newtonsoft.Json.Linq;

namespace NetworkPerspective.Sync.Application.Domain.Meetings
{
    public sealed class RecurrenceFactory
    {
        private const int DefaultInterval = 1;

        /// <summary>
        /// Creates <see cref="Recurrence"/> instance from RFC5545 RRULE compliant string"/>
        /// </summary>
        /// <param name="rrule">RFC5545 RRULE compliant string</param>
        /// <returns>Instance of <see cref="Recurrence"/></returns>
        public Recurrence CreateFromRRule(string rrule)
        {
            try
            {
                if (rrule == null)
                    return null;

                var parts = rrule.Split(new[] { ':', ';' });

                var frequency = GetValue(parts, "FREQ");
                var byDay = GetValue(parts, "BYDAY");
                var interval = GetValue(parts, "INTERVAL");

                var recurrenceType = StringToReocurrenceType(frequency);

                if (recurrenceType == RecurrenceType.Weekly)
                {
                    var daysCount = byDay?.Split(',').Length;

                    if (daysCount > 1)
                        recurrenceType = RecurrenceType.Daily;
                }

                return new Recurrence
                {
                    Type = recurrenceType,
                    Interval = interval == null ? DefaultInterval : int.Parse(interval),
                };
            }
            catch (Exception)
            {
                throw new UnexpectedRecurrenceFormatException(rrule);
            }
        }

        private static string GetValue(IEnumerable<string> parts, string key)
        {
            var value = parts.SingleOrDefault(x => x.StartsWith(key));

            if (value == null)
                return null;

            return value[key.Length..]
                .Trim('=');
        }

        private RecurrenceType StringToReocurrenceType(string input)
            => input switch
            {
                "YEARLY" => RecurrenceType.Yearly,
                "MONTHLY" => RecurrenceType.Monthly,
                "WEEKLY" => RecurrenceType.Weekly,
                "DAILY" => RecurrenceType.Daily,
                "HOURLY" => RecurrenceType.Hourly,
                "MINUTELY" => RecurrenceType.Minutely,
                "SECONDLY" => RecurrenceType.Secondly,
                _ => throw new ArgumentOutOfRangeException(nameof(RecurrenceType), $"'{input}' is not a valid {nameof(RecurrenceType)}"),
            };
    }
}