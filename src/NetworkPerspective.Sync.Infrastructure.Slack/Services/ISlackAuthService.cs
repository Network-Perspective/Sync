using System;
using System.Net;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Infrastructure.Slack.Client;
using NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos;
using NetworkPerspective.Sync.Infrastructure.Slack.Configs;
using NetworkPerspective.Sync.Infrastructure.Slack.Exceptions;
using NetworkPerspective.Sync.Infrastructure.Slack.Models;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Services
{
    public interface ISlackAuthService : IDisposable
    {
        Task<AuthStartProcessResult> StartAuthProcessAsync(AuthProcess authProcess, CancellationToken stoppingToken = default);
        Task HandleAuthorizationCodeCallbackAsync(string code, string state, CancellationToken stoppingToken = default);
    }

    internal class SlackAuthService : ISlackAuthService
    {
        private const int SlackAuthorizationCodeExpirationTimeInMinutes = 10;
        private readonly AuthConfig _slackAuthConfig;
        private readonly IStateKeyFactory _stateKeyFactory;
        private readonly ISecretRepositoryFactory _secretRepositoryFactory;
        private readonly ISlackClientFacade _slackClientFacade;
        private readonly IMemoryCache _cache;
        private readonly IStatusLoggerFactory _statusLoggerFactory;
        private readonly ILogger<SlackAuthService> _logger;

        public SlackAuthService(IStateKeyFactory stateFactory, IOptions<AuthConfig> slackAuthConfig, ISecretRepositoryFactory secretRepositoryFactory, ISlackClientFacade slackClientFacade, IMemoryCache cache, IStatusLoggerFactory statusLoggerFactory, ILogger<SlackAuthService> logger)
        {
            _slackAuthConfig = slackAuthConfig.Value;
            _stateKeyFactory = stateFactory;
            _secretRepositoryFactory = secretRepositoryFactory;
            _slackClientFacade = slackClientFacade;
            _cache = cache;
            _statusLoggerFactory = statusLoggerFactory;
            _logger = logger;
        }

        public void Dispose()
        {
            _slackClientFacade?.Dispose();
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

            var authUri = BuildSlackAuthUri(stateKey, authProcess.CallbackUri, clientId);

            _logger.LogInformation("Slack authentication process started. Unique state id: '{state}'", stateKey);

            return new AuthStartProcessResult(authUri);
        }

        public async Task HandleAuthorizationCodeCallbackAsync(string code, string state, CancellationToken stoppingToken = default)
        {
            _logger.LogInformation("Received Authentication callback.");

            if (!_cache.TryGetValue(state, out AuthProcess authProcess))
                throw new AuthException("State does not match initialized value");

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

            var response = await _slackClientFacade.AccessAsync(request);
            _slackClientFacade.SetAccessToken(response.AccessToken);

            // save the slack access key for this specific network
            var tokenKey = string.Format(SlackKeys.TokenKeyPattern, authProcess.NetworkId);
            await secretRepository.SetSecretAsync(tokenKey, response.AccessToken.ToSecureString());

            await _statusLoggerFactory
                .CreateForNetwork(authProcess.NetworkId)
                .LogInfoAsync("Connector authorized successfully", stoppingToken);
            _logger.LogInformation("Authentication callback processed successfully. Network '{networkId}' is configured for synchronization", authProcess.NetworkId);
        }

        private string BuildSlackAuthUri(string state, Uri callbackUrl, SecureString slackClientId)
        {
            _logger.LogDebug("Building slack auth path...");

            var uriBuilder = new UriBuilder("https://slack.com/oauth/v2/authorize");

            var scopesParameter = string.Join(',', _slackAuthConfig.Scopes);
            var userScopesParameter = string.Join(',', _slackAuthConfig.UserScopes);

            uriBuilder.Query = string.Format("redirect_uri={0}&client_id={1}&scope={2}&user_scope={3}&state={4}", callbackUrl.ToString(), slackClientId.ToSystemString(), scopesParameter, userScopesParameter, state);

            _logger.LogDebug("Built slack auth path: '{uriBuilder}'", uriBuilder);

            return uriBuilder.ToString();
        }
    }
}