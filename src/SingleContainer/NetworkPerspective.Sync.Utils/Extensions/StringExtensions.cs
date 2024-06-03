using System.Net;
using System.Security;

namespace NetworkPerspective.Sync.Utils.Extensions;

public static class StringExtensions
{
    public static SecureString ToSecureString(this string str)
        => new NetworkCredential(string.Empty, str).SecurePassword;
}