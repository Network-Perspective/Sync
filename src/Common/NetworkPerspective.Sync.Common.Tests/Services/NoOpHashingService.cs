using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Common.Tests.Services;

public class NoOpHashingService : IHashingService
{
    public string Hash(string input)
        => input;
}