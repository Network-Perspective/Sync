using System;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Exceptions
{
    internal class TooManyMailsPerUserException : Exception
    {
        public string Email { get; }

        public TooManyMailsPerUserException(string email) : base($"Maximum allowed messages limit exceeded for user '{email}'")
        {
            Email = email;
        }
    }
}