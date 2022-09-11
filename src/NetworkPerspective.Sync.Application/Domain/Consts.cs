using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Application.Domain
{
    public static class Consts
    {
        public static readonly IEqualityComparer<string> UserIdEqualityComparer = StringComparer.InvariantCultureIgnoreCase;
    }
}