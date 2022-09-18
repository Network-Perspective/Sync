using System;

namespace NetworkPerspective.Sync.Application.Exceptions
{
    public class DoubleHashingException : Exception
    {
        public DoubleHashingException(string propertyName)
            : base($"{propertyName} is already hashed. Hashing twice is just silly... isn't it?")
        { }
    }
}