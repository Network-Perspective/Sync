using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkPerspective.Sync.Application.Domain.Networks.Filters
{
    public class EmployeeFilter
    {
        private const string EmailWhitelistPrefix = "email:";
        private const string GroupWhitelistPrefix = "group:";

        public static readonly EmployeeFilter Empty = new EmployeeFilter(Array.Empty<string>(), Array.Empty<string>());

        private readonly Blacklist _emailBlacklist;
        private readonly Whitelist _emailWhitelist;
        private readonly Whitelist _groupWhitelist;

        public EmployeeFilter(IEnumerable<string> whitelist, IEnumerable<string> blacklist)
        {
            if (whitelist is null || !whitelist.Any())
                whitelist = new[] { $"{EmailWhitelistPrefix}*" };

            var emailWhitelist = whitelist
                .Where(x => x.StartsWith(EmailWhitelistPrefix)) // with email prefix
                .Select(x => x[EmailWhitelistPrefix.Length..])  // remove the prefix
                .Select(x => x.Trim())                          // trim whitespaces
                .Where(x => !string.IsNullOrEmpty(x));          // remove empty entries

            // Only for backward compatibility, once all networks are configured to use prefixes (email: / group:) should be removed
            var emailLegacyWhitelist = whitelist
                .Where(x => !x.StartsWith(EmailWhitelistPrefix) && !x.StartsWith(GroupWhitelistPrefix));

            var groupWhitelist = whitelist
                .Where(x => x.StartsWith(GroupWhitelistPrefix)) // with group prefix
                .Select(x => x[GroupWhitelistPrefix.Length..])  // remove the prefix
                .Select(x => x.Trim())                          // trim whitespaces
                .Where(x => !string.IsNullOrEmpty(x));          // remove empty entries

            _emailWhitelist = new Whitelist(emailWhitelist.Union(emailLegacyWhitelist));
            _groupWhitelist = new Whitelist(groupWhitelist);
            _emailBlacklist = new Blacklist(blacklist);
        }

        public bool IsInternal(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            if (_emailBlacklist.IsForbidden(email))
                return false;

            return _emailWhitelist.IsAllowed(email);
        }

        public bool IsInternal(string email, string group)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            if (_emailBlacklist.IsForbidden(email))
                return false;

            return _emailWhitelist.IsAllowed(email) || _groupWhitelist.IsAllowed(group);
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine($"Whitelist groups: {_groupWhitelist.ToString()};");
            stringBuilder.AppendLine($"Whitelist emails: {_emailWhitelist.ToString()};");
            stringBuilder.AppendLine($"Blacklist emails: {_emailBlacklist.ToString()};");

            return stringBuilder.ToString();
        }
    }
}