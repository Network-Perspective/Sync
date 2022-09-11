using System;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
    public interface IStateKeyFactory
    {
        string Create();
    }

    internal class StateKeyFactory : IStateKeyFactory
    {
        public string Create()
            => Guid.NewGuid().ToString();
    }
}