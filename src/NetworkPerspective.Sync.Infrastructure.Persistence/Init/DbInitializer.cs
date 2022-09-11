using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Infrastructure.Persistence;

namespace NetworkPerspective.Sync.Infrastructure.Persistence.Init
{
    public class DbInitializer : IDbInitializer
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private readonly ILogger<DbInitializer> _logger;

        public DbInitializer(IUnitOfWorkFactory unitOfWorkFactory, ILogger<DbInitializer> logger)
        {
            _unitOfWorkFactory = unitOfWorkFactory;
            _logger = logger;
        }

        public async Task InitializeAsync()
        {
            try
            {
                using var unitOfWork = _unitOfWorkFactory.Create();
                await unitOfWork.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unable to initialize database");
                throw;
            }
        }
    }
}