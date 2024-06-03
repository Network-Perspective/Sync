using System;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Mappers
{
    // Please do not try optimize it with DateTimeOffset.FromUnixTimeSeconds.. it's not precise enough!
    public static class TimeStampMapper
    {
        private static readonly long BaseEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        public static DateTime SlackTimeStampToDateTime(long timestamp)
            => SlackTimeStampToDateTime(timestamp.ToString());

        public static DateTime SlackTimeStampToDateTime(string timestamp)
        {
            var epoch = decimal.Parse(timestamp);
            var epochTicks = BaseEpochTicks + epoch * TimeSpan.TicksPerSecond;
            var dateTime = new DateTime((long)epochTicks);

            return dateTime;
        }

        public static string DateTimeToSlackTimeStamp(DateTime dateTime)
        {
            var elapsedTicks = dateTime.Ticks - BaseEpochTicks;

            var slackTimeStamp = (elapsedTicks / (double)TimeSpan.TicksPerSecond).ToString("F6");

            return slackTimeStamp;
        }
    }
}