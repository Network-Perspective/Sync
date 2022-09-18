using System;
using System.Collections.Generic;
using System.Linq;

using Google.Apis.Gmail.v1.Data;

using NetworkPerspective.Sync.Application.Services;

namespace NetworkPerspective.Sync.Infrastructure.Google.Extensions
{
    internal static class MessageExtensions
    {
        public static string GetSender(this Message message)
        {
            var headerValue = message.GetHeaderValue("from");
            var emails = headerValue.GetUserEmails();

            if (emails.Length == 1)
                return emails[0];
            else
                return "empty";
        }

        public static ISet<string> GetReceivers(this Message message)
        {
            var receivers = message.GetHeaderValue("to").GetUserEmails();
            var copyReceivers = message.GetHeaderValue("cc").GetUserEmails();
            var blindCopyReceivers = message.GetHeaderValue("bcc").GetUserEmails();

            var result = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

            result.UnionWith(receivers);
            result.UnionWith(copyReceivers);
            result.UnionWith(blindCopyReceivers);

            return result;
        }

        public static DateTime GetDateTime(this Message message, IClock clock)
        {
            if (message.InternalDate.HasValue)
                return DateTimeOffset.FromUnixTimeMilliseconds(message.InternalDate.Value).UtcDateTime;
            else
                return clock.UtcNow();
        }

        public static string GetHeaderValue(this Message message, string headerName)
        {
            var headerNameEqualityComparer = StringComparer.InvariantCultureIgnoreCase;
            var header = message.Payload.Headers.FirstOrDefault(x => headerNameEqualityComparer.Equals(x.Name, headerName));

            if (header == null)
                return string.Empty;
            else
                return header.Value;
        }
    }
}