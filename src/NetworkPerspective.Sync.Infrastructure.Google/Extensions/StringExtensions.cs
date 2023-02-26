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
            if (string.IsNullOrEmpty(headerValue))
                return Array.Empty<string>();

            if (ContainsGroup(headerValue))
                return Array.Empty<string>();

            var mailAddressessCollection = new MailAddressCollection();

            try
            {
                mailAddressessCollection.Add(headerValue);
            }

            catch
            {
                throw new NotSupportedEmailFormatException(headerValue);
            }

            return mailAddressessCollection
                .Select(x => x.Address)
                .ToArray();
        }

        public static string[] ExtractEmailAddress(this string[] conversationParticipant)
        {
            return conversationParticipant
                .Where(DoesntContainGroup)
                .Select(ExtractSingleEmailAddress)
                .ToArray();
        }

        private static string ExtractSingleEmailAddress(string email)
        {
            try
            {
                return new MailAddress(email).Address;
            }
            catch (Exception)
            {
                throw new NotSupportedEmailFormatException(email);
            }
        }

        private static bool DoesntContainGroup(string input)
            => !ContainsGroup(input);

        private static bool ContainsGroup(string input)
            => input.Contains(':') && input.Contains(';');
    }
}