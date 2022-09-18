using System;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IClock
    {
        DateTime UtcNow();
    }

    public class Clock : IClock
    {
        public DateTime UtcNow()
            => DateTime.UtcNow;
    }
}