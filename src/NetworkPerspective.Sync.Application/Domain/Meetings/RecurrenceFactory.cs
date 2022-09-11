using System;
using System.Linq;

using NetworkPerspective.Sync.Application.Exceptions;

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

                var frequency = parts.Single(x => x.StartsWith("FREQ"));
                var interval = parts.SingleOrDefault(x => x.StartsWith("INTERVAL"));

                return new Recurrence
                {
                    Type = StringToReocurrenceType(frequency[5..]),
                    Interval = interval == null ? DefaultInterval : int.Parse(interval[9..]),
                };
            }
            catch (Exception)
            {
                throw new UnexpectedRecurrenceFormatException(rrule);
            }
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