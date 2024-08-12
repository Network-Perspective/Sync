namespace NetworkPerspective.Sync.Worker.Application.Exceptions
{
    public class DoubleHashingException : ApplicationException
    {
        public DoubleHashingException(string propertyName)
            : base($"{propertyName} is already hashed. Hashing twice is just silly... isn't it?")
        { }
    }
}