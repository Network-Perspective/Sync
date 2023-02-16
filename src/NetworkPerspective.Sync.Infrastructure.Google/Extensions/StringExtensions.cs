using System;
using System.Linq;
using System.Net.Mail;

namespace NetworkPerspective.Sync.Infrastructure.Google.Extensions
{
    internal static class StringExtensions
    {
        public static string[] GetUserEmails(this string value)
        {
            if (value is null)
                return Array.Empty<string>();

            var participantsArray = value.Split(", ", StringSplitOptions.RemoveEmptyEntries);
            return ExtractEmailAddress(participantsArray);
        }

        public static string[] ExtractEmailAddress(this string[] conversationParticipant)
            => conversationParticipant
                .Select(x => new MailAddress(x).Address)
                .ToArray();
    }
}