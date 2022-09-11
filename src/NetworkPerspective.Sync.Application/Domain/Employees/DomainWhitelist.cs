using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class DomainWhitelist
    {
        private readonly IEnumerable<string> _allowedDomainsRegexExpressions;

        public DomainWhitelist(IEnumerable<string> allowedDomains)
        {
            if (allowedDomains == null)
                _allowedDomainsRegexExpressions = Array.Empty<string>();
            else
                _allowedDomainsRegexExpressions = allowedDomains.Select(WildCardToRegular);
        }

        public bool IsInAllowedDomain(string email)
        {
            if (!_allowedDomainsRegexExpressions.Any())
                return true;

            return _allowedDomainsRegexExpressions.Any(x => Regex.IsMatch(email, x));
        }

        private static string WildCardToRegular(string value)
        {
            var innerPart = Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*");
            return $"^{innerPart}$";
        }
    }
}