using Google.Apis.Auth.OAuth2.Responses;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Extensions
{
    internal static class TokenErrorResponseExtensions
    {
        public static bool IsInvalidSignatureError(this TokenErrorResponse error)
            => error.Error == "invalid_grant" && error.ErrorDescription.StartsWith("java.security.SignatureException");
    }
}