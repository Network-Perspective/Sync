﻿using System.Security;

using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Extensions;

internal static class HttpRequestExtensions
{
    public static SecureString GetServiceAccessToken(this HttpRequest request)
    {
        var value = request.Headers[HeaderNames.Authorization].ToString();

        if (string.IsNullOrEmpty(value))
            throw new MissingAuthorizationHeaderException(HeaderNames.Authorization);

        return value
            .Replace("Bearer", string.Empty)
            .Trim()
            .ToSecureString();
    }
}