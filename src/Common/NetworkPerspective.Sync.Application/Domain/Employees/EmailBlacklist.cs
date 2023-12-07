using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class EmailBlacklist
    {
        private readonly IEnumerable<string> _forbiddenEmails;

        public EmailBlacklist(IEnumerable<string> forbiddenEmails)
        {
            _forbiddenEmails = forbiddenEmails ?? Array.Empty<string>();
        }

        public bool IsForbiddenEmail(string email)
            => _forbiddenEmails.Contains(email);

        public override string ToString()
            => _forbiddenEmails.Any()
            ? string.Join(", ", _forbiddenEmails)
            : "<empty>";
    }
}