using System.Net;
using System.Security;

namespace NetworkPerspective.Sync.Application.Extensions
{
    public static class SecureStringExtensions
    {
        public static string ToSystemString(this SecureString secureString)
            => new NetworkCredential(string.Empty, secureString).Password;
    }
}