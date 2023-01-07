using System;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IStatusLoggerFactory
    {
        IStatusLogger CreateForNetwork(Guid networkId);
    }

    internal class StatusLoggerFactory : IStatusLoggerFactory
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly IClock _clock;
        private readonly ILoggerFactory _loggerFactory;

        public StatusLoggerFactory(IUnitOfWorkFactory unitOfWorkFactory, IClock clock, ILoggerFactory loggerFactory)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _clock = clock;
            _loggerFactory = loggerFactory;
        }

        public IStatusLogger CreateForNetwork(Guid networkId)
            => new StatusLogger(networkId, _unitOfWorkFactory, _clock, _loggerFactory.CreateLogger<StatusLogger>());
    }
}