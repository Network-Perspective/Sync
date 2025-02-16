using System;
using System.Net;
using System.Security;

namespace NetworkPerspective.Sync.Utils.Extensions;

public static class StringExtensions
{
    public static SecureString ToSecureString(this string str)
        => new NetworkCredential(string.Empty, str).SecurePassword;

    public static string Sanitize(this string str)
    {
        if (string.IsNullOrEmpty(str))
            return str;

        return str
                .ToStringWithoutControlCharacters()
                .ToEscapedString();
    }

    private static string ToStringWithoutControlCharacters(this string str)
        => str
            .Replace(Environment.NewLine, string.Empty)
            .Replace("\n", string.Empty)
            .Replace("\r", string.Empty)
            .Replace("\t", string.Empty);

    private static string ToEscapedString(this string str)
        => SecurityElement.Escape(str);
}