using System;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Application.Services
{
    public interface IStatusLoggerFactory
    {
        IStatusLogger CreateForConnector(Guid connectorId);
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

        public IStatusLogger CreateForConnector(Guid connectorInfo)
            => new StatusLogger(connectorInfo, _unitOfWorkFactory, _clock, _loggerFactory.CreateLogger<StatusLogger>());
    }
}