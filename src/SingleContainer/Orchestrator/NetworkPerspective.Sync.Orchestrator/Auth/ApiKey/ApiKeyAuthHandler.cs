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

using NetworkPerspective.Sync.Orchestrator.Application.Services;
using NetworkPerspective.Sync.Orchestrator.Extensions;
using NetworkPerspective.Sync.Orchestrator.Infrastructure.Vault.Contract;
using NetworkPerspective.Sync.Utils.Extensions;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Orchestrator.Auth.ApiKey;

public class ApiKeyAuthHandler : AuthenticationHandler<ApiKeyAuthOptions>
{

    private const string AuthenticationExceptionKey = "ApiKeyAuthenticationException";

    private readonly IVault _vault;
    private readonly IErrorService _errorService;
    private readonly IOptionsMonitor<ApiKeyAuthOptions> _options;
    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore
    };

    public ApiKeyAuthHandler(IVault vault, IErrorService errorService, IOptionsMonitor<ApiKeyAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder) : base(options, logger, encoder)
    {
        _vault = vault;
        _errorService = errorService;
        _options = options;
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
        var expectedKey = await _vault.GetSecretAsync(_options.CurrentValue.ApiKeyVaultKey, Context.RequestAborted);
        var actualKey = Request.GetBearerToken();

        if (!string.Equals(expectedKey.ToSystemString(), actualKey.ToSystemString()))
            return AuthenticateResult.Fail("Api key is not valid");

        var claims = new List<Claim>()
        { };
        var claimsIdentity = new ClaimsIdentity(claims, Scheme.Name);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        return AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name));
    }

    private static bool AllowAnonymousAccess(HttpContext context)
        => context.GetEndpoint()?.Metadata?.GetMetadata<IAllowAnonymous>() is not null;
}