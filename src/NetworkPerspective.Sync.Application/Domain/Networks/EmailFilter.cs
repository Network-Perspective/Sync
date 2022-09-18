using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Application.Domain.Networks
{
    public class EmailFilter
    {
        public static readonly EmailFilter Empty = new EmailFilter(Array.Empty<string>(), Array.Empty<string>());

        private readonly EmailBlacklist _emailBlacklist;
        private readonly DomainWhitelist _domainWhitelist;

        public EmailFilter(IEnumerable<string> allowedDomains, IEnumerable<string> forbiddenEmails)
        {
            _domainWhitelist = new DomainWhitelist(allowedDomains);
            _emailBlacklist = new EmailBlacklist(forbiddenEmails);
        }

        public bool IsInternalUser(string email)
        {
            if (string.IsNullOrEmpty(email))
                return false;

            if (_emailBlacklist.IsForbiddenEmail(email))
                return false;

            return _domainWhitelist.IsInAllowedDomain(email);
        }
    }
}