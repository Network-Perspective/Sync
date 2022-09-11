using System;

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
        {
            var array = new string[conversationParticipant.Length];

            for (int i = 0; i < conversationParticipant.Length; i++)
            {
                var oryginalValue = conversationParticipant[i];
                var startBraceIndex = oryginalValue.IndexOf('<');
                var endBraceIndex = oryginalValue.IndexOf('>');

                if (startBraceIndex != -1 && endBraceIndex != -1)
                    array[i] = oryginalValue[(startBraceIndex + 1)..endBraceIndex];
                else
                    array[i] = oryginalValue;
            }

            return array;
        }
    }
}