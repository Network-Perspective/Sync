using System;

namespace NetworkPerspective.Sync.Application.Infrastructure.Persistence.Exceptions
{
    public class DbException : Exception
    {
        public DbException(string message) : base(message)
        { }

        public DbException(Exception innerException) : base("General Db exception has been thrown. Please see inner exception", innerException)
        { }
    }
}