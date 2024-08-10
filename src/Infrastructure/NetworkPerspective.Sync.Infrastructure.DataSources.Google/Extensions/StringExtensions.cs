using System;
using System.Linq;

using MimeKit;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Exceptions;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Extensions
{
    internal static class StringExtensions
    {
        public static string[] GetUserEmails(this string headerValue)
        {
            if (string.IsNullOrEmpty(headerValue))
                return Array.Empty<string>();

            if (InternetAddressList.TryParse(headerValue, out var list))
                return list.Mailboxes.Select(x => x.Address).ToArray();
            else
                throw new NotSupportedEmailFormatException(headerValue);
        }

        public static string[] ExtractEmailAddress(this string[] conversationParticipant)
        {
            return conversationParticipant
                .SelectMany(GetUserEmails)
                .ToArray();
        }
    }
}