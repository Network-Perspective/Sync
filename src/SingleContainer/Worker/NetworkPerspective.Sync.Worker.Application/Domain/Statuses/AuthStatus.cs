using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Statuses;

public class AuthStatus
{
    public static readonly AuthStatus Empty = Create(true);

    public bool IsAuthorized { get; }
    public IDictionary<string, string> Properties { get; }


    private AuthStatus(bool isAuthorized, IDictionary<string, string> properties)
    {
        IsAuthorized = isAuthorized;
        Properties = properties;
    }

    public static AuthStatus Create(bool isAuthorized)
        => new(isAuthorized, ImmutableDictionary<string, string>.Empty);

    public static AuthStatus WithProperties(bool isAuthorized, IDictionary<string, string> properties)
        => new(isAuthorized, properties);
}