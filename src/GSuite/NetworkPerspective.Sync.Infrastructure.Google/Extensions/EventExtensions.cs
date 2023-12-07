using System;
using System.Collections.Generic;
using System.Linq;

using Google.Apis.Calendar.v3.Data;

using NetworkPerspective.Sync.Application.Domain.Meetings;

namespace NetworkPerspective.Sync.Infrastructure.Google.Extensions
{
    internal static class EventExtensions
    {
        private static readonly RecurrenceFactory RecurrenceFactory = new RecurrenceFactory();

        public static int GetDurationInMinutes(this Event @event)
        {
            if (@event.Start?.DateTime is null || @event.End?.DateTime is null)
                return 0;
            else
                return (int)(@event.End.DateTime.Value - @event.Start.DateTime.Value).TotalMinutes;
        }

        public static DateTime GetStart(this Event @event)
            => @event.Start?.DateTime ?? DateTime.UtcNow;

        public static IEnumerable<string> GetParticipants(this Event @event)
        {
            if (@event.Attendees is null)
                return Array.Empty<string>();

            var participants = @event.Attendees.Select(x => x.Email).ToArray();
            return participants.ExtractEmailAddress();
        }

        public static RecurrenceType? GetRecurrence(this Event @event)
        {
            if (@event.Recurrence is null)
                return null;

            var rrule = @event.Recurrence.FirstOrDefault(x => x.StartsWith("RRULE"));

            return RecurrenceFactory.CreateFromRRule(rrule).Type;
        }
    }
}