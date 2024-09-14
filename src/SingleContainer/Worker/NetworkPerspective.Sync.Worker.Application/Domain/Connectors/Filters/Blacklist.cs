using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Connectors.Filters
{
    public class Blacklist
    {
        private readonly IEnumerable<string> _forbiddenList;

        public Blacklist(IEnumerable<string> forbiddenList)
        {
            _forbiddenList = forbiddenList ?? Array.Empty<string>();
        }

        public bool IsForbidden(string input)
            => _forbiddenList.Contains(input);

        public override string ToString()
            => _forbiddenList.Any()
            ? string.Join(", ", _forbiddenList)
            : "<empty>";
    }
}