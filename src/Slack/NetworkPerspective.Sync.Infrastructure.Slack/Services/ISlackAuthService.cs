using System;
using System.Linq;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Configs;
using NetworkPerspective.Sync.Infrastructure.Slack.Models;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
    public interface ISlackAuthService
    {
        Task<AuthStartProcessResult> StartAuthProcessAsync(AuthProcess authProcess, CancellationToken stoppingToken = default);
        Task HandleAuthorizationCodeCallbackAsync(string code, string state, CancellationToken stoppingToken = default);
    }

    internal class SlackAuthService : ISlackAuthService
    {
        private const int SlackAuthorizationCodeExpirationTimeInMinutes = 10;

        private readonly AuthConfig _slackAuthConfig;
        private readonly IAuthStateKeyFactory _stateKeyFactory;
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;
        private readonly ISlackClientUnauthorizedFacade _slackClientUnauthorizedFacade;
        private readonly IMemoryCache _cache;
        private readonly IStatusLoggerFactory _statusLoggerFactory;
        private readonly ILogger<SlackAuthService> _logger;

        public SlackAuthService(
            IAuthStateKeyFactory stateFactory,
            IOptions<AuthConfig> slackAuthConfig,
            ISecretRepositoryFactory secretRepositoryFactory,
            ISlackClientUnauthorizedFacade slackClientUnauthorizedFacade,
            IMemoryCache cache,
            IStatusLoggerFactory statusLoggerFactory,
            ILogger<SlackAuthService> logger)
        {
            _slackAuthConfig = slackAuthConfig.Value;
            _stateKeyFactory = stateFactory;
            _secretRepositoryFactory = secretRepositoryFactory;
            _slackClientUnauthorizedFacade = slackClientUnauthorizedFacade;
            _cache = cache;
            _statusLoggerFactory = statusLoggerFactory;
            _logger = logger;
        }

        public async Task<AuthStartProcessResult> StartAuthProcessAsync(AuthProcess authProcess, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Starting slack autentication process...");
            await _statusLoggerFactory
                .CreateForNetwork(authProcess.NetworkId)
                .LogInfoAsync("Authorization process started", stoppingToken);

            var secretRepository = await _secretRepositoryFactory.CreateAsync(authProcess.NetworkId, stoppingToken);
            var clientId = await secretRepository.GetSecretAsync(SlackKeys.SlackClientIdKey, stoppingToken);

            var stateKey = _stateKeyFactory.Create();
            _cache.Set(stateKey, authProcess, DateTimeOffset.UtcNow.AddMinutes(SlackAuthorizationCodeExpirationTimeInMinutes));

            var authUri = BuildSlackAuthUri(stateKey, authProcess, clientId);

            _logger.LogInformation("Slack authentication process started. Unique state id: '{state}'", stateKey);

            return new AuthStartProcessResult(authUri);
        }

        public async Task HandleAuthorizationCodeCallbackAsync(string code, string state, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Received Authentication callback.");

            if (!_cache.TryGetValue(state, out AuthProcess authProcess))
                throw new OAuthException("State does not match initialized value");

            var secretRepository = await _secretRepositoryFactory.CreateAsync(authProcess.NetworkId, stoppingToken);

            var clientId = await secretRepository.GetSecretAsync(SlackKeys.SlackClientIdKey, stoppingToken);
            var clientSecret = await secretRepository.GetSecretAsync(SlackKeys.SlackClientSecretKey, stoppingToken);

            var request = new OAuthAccessRequest
            {
                ClientId = clientId.ToSystemString(),
                ClientSecret = clientSecret.ToSystemString(),
                GrantType = "authorization_code",
                RedirectUri = authProcess.CallbackUri.ToString(),
                Code = code,
            };

            var response = await _slackClientUnauthorizedFacade.AccessAsync(request, stoppingToken);

            var tokenKey = string.Format(SlackKeys.TokenKeyPattern, authProcess.NetworkId);
            await secretRepository.SetSecretAsync(tokenKey, response.AccessToken.ToSecureString(), stoppingToken);

            // save refresh token if token rotation is enabled
            if (!string.IsNullOrEmpty(response.RefreshToken))
            {
                var refreshTokenKey = string.Format(SlackKeys.RefreshTokenPattern, authProcess.NetworkId);
                await secretRepository.SetSecretAsync(refreshTokenKey, response.RefreshToken.ToSecureString(),
                    stoppingToken);
            }

            if (authProcess.RequireAdminPrivileges)
            {
                var userTokenKey = string.Format(SlackKeys.UserTokenKeyPattern, authProcess.NetworkId);
                await secretRepository.SetSecretAsync(userTokenKey, response.User.AccessToken.ToSecureString(), stoppingToken);
            }

            await _statusLoggerFactory
                .CreateForNetwork(authProcess.NetworkId)
                .LogInfoAsync("Connector authorized successfully", stoppingToken);
            _logger.LogInformation("Authentication callback processed successfully. Network '{networkId}' is configured for synchronization", authProcess.NetworkId);
        }

        private string BuildSlackAuthUri(string state, AuthProcess authProcess, SecureString slackClientId)
        {
            _logger.LogDebug("Building slack auth path... The {parameter} is set to '{value}'", nameof(authProcess.RequireAdminPrivileges), authProcess.RequireAdminPrivileges);

            var uriBuilder = new UriBuilder("https://slack.com/oauth/v2/authorize");

            var scopesParameter = string.Join(',', _slackAuthConfig.Scopes);

            var userScopes = authProcess.RequireAdminPrivileges
                ? _slackAuthConfig.UserScopes.Union(_slackAuthConfig.AdminUserScopes)
                : _slackAuthConfig.UserScopes;
            var userScopesParameter = string.Join(',', userScopes);

            uriBuilder.Query = string.Format("redirect_uri={0}&client_id={1}&scope={2}&user_scope={3}&state={4}", authProcess.CallbackUri.ToString(), slackClientId.ToSystemString(), scopesParameter, userScopesParameter, state);

            _logger.LogDebug("Built slack auth path: '{uriBuilder}'", uriBuilder);

            return uriBuilder.ToString();
        }
    }
}