using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Extensions;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Framework.Auth
{
    public class ServiceAuthHandler : AuthenticationHandler<ServiceAuthOptions>
    {
        private const string AuthenticationExceptionKey = "AuthenticationException";
        private readonly INetworkPerspectiveCore _networkPerspectiveCore;
        private readonly IConnectorInfoInitializer _connectorInfoInitializer;
        private readonly IErrorService _errorService;

        private readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            Formatting = Formatting.Indented,
            NullValueHandling = NullValueHandling.Ignore,
            DefaultValueHandling = DefaultValueHandling.Ignore
        };

        public ServiceAuthHandler(IOptionsMonitor<ServiceAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, INetworkPerspectiveCore networkPerspectiveCore, IConnectorInfoInitializer connectorInfoInitializer, IErrorService errorService)
            : base(options, logger, encoder)
        {
            _networkPerspectiveCore = networkPerspectiveCore;
            _connectorInfoInitializer = connectorInfoInitializer;
            _errorService = errorService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (AllowAnonymousAccess(Context))
                return AuthenticateResult.NoResult();

            try
            {
                return await AuthenticateInternalAsync();
            }
            catch (Exception ex)
            {
                Context.Items.Add(AuthenticationExceptionKey, ex);
                return AuthenticateResult.Fail(ex);
            }
        }

        protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
        {
            var ex = Context.Items[AuthenticationExceptionKey] as Exception;

            if (ex is null)
                await base.HandleChallengeAsync(properties);
            else
            {
                var error = _errorService.MapToError(ex);
                var problemDetails = new ProblemDetails
                {
                    Type = error.Type,
                    Title = error.Title,
                    Detail = error.Details,
                    Status = error.StatusCode
                };

                Context.Response.StatusCode = 401;
                await Context.Response.WriteAsync(JsonConvert.SerializeObject(problemDetails, _jsonSerializerSettings));
            }
        }

        private async Task<AuthenticateResult> AuthenticateInternalAsync()
        {
            var connectorInfo = await _networkPerspectiveCore.ValidateTokenAsync(Request.GetServiceAccessToken(), Context.RequestAborted);

            _connectorInfoInitializer.Initialize(connectorInfo);

            var claims = new List<Claim>()
                {
                    new("NetworkId", connectorInfo.NetworkId.ToString()),
                    new("ConnectorId", connectorInfo.Id.ToString())
                };
            var claimsIdentity = new ClaimsIdentity(claims, Scheme.Name);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            return AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name));
        }

        private static bool AllowAnonymousAccess(HttpContext context)
            => context.GetEndpoint()?.Metadata?.GetMetadata<IAllowAnonymous>() is not null;
    }
}