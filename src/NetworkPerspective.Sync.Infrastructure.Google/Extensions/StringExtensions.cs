using System;
using System.Linq;
using System.Net.Mail;

using NetworkPerspective.Sync.Infrastructure.Google.Exceptions;

namespace NetworkPerspective.Sync.Infrastructure.Google.Extensions
{
    internal static class StringExtensions
    {
        public static string[] GetUserEmails(this string headerValue)
        {
            if (headerValue is null)
                return Array.Empty<string>();

            var participantsArray = headerValue.Split(", ", StringSplitOptions.RemoveEmptyEntries);
            return ExtractEmailAddress(participantsArray);
        }

        public static string[] ExtractEmailAddress(this string[] conversationParticipant)
        {
            return conversationParticipant
                .Where(x => !x.Contains(':')) // Group
                .Select(ExtractSingleEmailAddress)
                .ToArray();
        }

        private static string ExtractSingleEmailAddress(string email)
        {
            try
            {
                return new MailAddress(email).Address;
            }
            catch(Exception)
            {
                throw new NotSupportedEmailFormatException(email);
            }
        }
    }
}