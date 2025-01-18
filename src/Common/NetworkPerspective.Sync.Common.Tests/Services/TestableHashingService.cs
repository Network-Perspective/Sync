using NetworkPerspective.Sync.Worker.Application.Domain;
using NetworkPerspective.Sync.Worker.Application.Services;

namespace NetworkPerspective.Sync.Common.Tests.Services;

public class TestableHashingService(HashFunction.Delegate hashFunction) : IHashingService
{
    public string Hash(string input)
        => hashFunction(input);
}