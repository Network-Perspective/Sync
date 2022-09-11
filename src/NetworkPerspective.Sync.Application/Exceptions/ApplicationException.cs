using System;

namespace NetworkPerspective.Sync.Application.Exceptions
{
    public class ApplicationException : Exception
    {
        public ApplicationException(string message) : base(message)
        { }
    }
}