﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters
{
    public class Whitelist
    {
        private readonly IEnumerable<string> _allowedRegexExpressions;

        public Whitelist(IEnumerable<string> allowedPatterns)
        {
            if (allowedPatterns == null)
                _allowedRegexExpressions = Array.Empty<string>();
            else
                _allowedRegexExpressions = allowedPatterns.Select(WildCardToRegular);
        }

        public bool IsAllowed(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;

            return _allowedRegexExpressions.Any(x => Regex.IsMatch(input, x));
        }

        private static string WildCardToRegular(string value)
        {
            var innerPart = Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*");
            return $"^{innerPart}$";
        }

        public override string ToString()
            => _allowedRegexExpressions.Any()
            ? string.Join(", ", _allowedRegexExpressions)
            : "<empty>";
    }
}