using System;

namespace NetworkPerspective.Sync.Infrastructure.Google.Exceptions
{
    internal class NotSupportedEmailFormatException : Exception
    {
        public string Email { get; }

        public NotSupportedEmailFormatException(string email) :
            base($"Provided email has not supported format: '{email}'")
        {
            Email = email;
        }
    }
}
