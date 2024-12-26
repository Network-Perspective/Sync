namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Model;

internal class TokenResponse(string accessToken, string refreshToken)
{
    public string AccessToken { get; } = accessToken;
    public string RefreshToken { get; } = refreshToken;
}