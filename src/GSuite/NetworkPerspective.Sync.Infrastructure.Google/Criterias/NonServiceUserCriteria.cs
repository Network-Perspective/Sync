using System.Collections.Generic;
using System.Linq;

using Google.Apis.Admin.Directory.directory_v1.Data;

using Microsoft.Extensions.Logging;

namespace NetworkPerspective.Sync.Infrastructure.Google.Criterias
{
    internal class NonServiceUserCriteria : ICriteria
    {
        private readonly ILogger<NonServiceUserCriteria> _logger;

        public NonServiceUserCriteria(ILogger<NonServiceUserCriteria> logger)
        {
            _logger = logger;
        }

        public IList<User> MeetCriteria(IList<User> input)
        {
            _logger.LogDebug("Filtering out service users. Input has {count} users", input.Count);

            var result = input
                .Where(HasGivenName)
                .Where(HasFamilyName)
                .ToList();

            _logger.LogDebug("There are {count} users with name assigned", result.Count);

            _logger.LogDebug("Filtering bot users completed. Output has {count} users", result.Count);

            return result;
        }

        private static bool HasGivenName(User user)
            => !string.IsNullOrEmpty(user.Name?.GivenName);

        private static bool HasFamilyName(User user)
            => !string.IsNullOrEmpty(user.Name?.FamilyName);
    }
}