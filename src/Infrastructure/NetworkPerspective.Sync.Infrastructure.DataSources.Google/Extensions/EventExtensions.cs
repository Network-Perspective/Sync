using System;
using System.Collections.Generic;
using System.Linq;

using Google.Apis.Calendar.v3.Data;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Exceptions;
using NetworkPerspective.Sync.Worker.Application.Domain.Meetings;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Extensions
{
    internal static class EventExtensions
    {
        private static readonly RecurrenceFactory RecurrenceFactory = new RecurrenceFactory();

        public static int GetDurationInMinutes(this Event @event)
        {
            if (@event.Start?.DateTimeDateTimeOffset is null || @event.End?.DateTimeDateTimeOffset is null)
                return 0;
            else
                return (int)(@event.End.DateTimeDateTimeOffset.Value - @event.Start.DateTimeDateTimeOffset.Value).TotalMinutes;
        }

        public static DateTime GetStart(this Event @event)
        {
            var start = @event.Start?.DateTimeDateTimeOffset;

            if (start is null)
                throw new MissingMeetingStartException();

            return start.Value.UtcDateTime;
        }

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