using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
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
using NetworkPerspective.Sync.Utils.Extensions;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Orchestrator.Auth.Worker;

public class WorkerAuthHandler : AuthenticationHandler<WorkerAuthOptions>
{
    private const string AuthenticationExceptionKey = "WorkerAuthenticationException";
    private readonly IWorkersService _workersService;
    private readonly IErrorService _errorService;

    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
        DefaultValueHandling = DefaultValueHandling.Ignore
    };

    public WorkerAuthHandler(IOptionsMonitor<WorkerAuthOptions> options, IWorkersService workersService, ILoggerFactory logger, UrlEncoder encoder, IErrorService errorService)
        : base(options, logger, encoder)
    {
        _workersService = workersService;
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
        var basicAuth = Request
            .GetBearerToken()
            .ToSystemString();
        var basicAuthByteArray = Convert.FromBase64String(basicAuth);
        var basicAuthString = Encoding.UTF8.GetString(basicAuthByteArray);
        var credentialsArray = basicAuthString.Split(':');
        var username = credentialsArray[0];
        var password = credentialsArray[1];

        var worker = await _workersService.AuthenticateAsync(username, password, Context.RequestAborted);

        var claims = new List<Claim>()
        {
            new(ClaimTypes.Name, worker.Name),
        };
        var claimsIdentity = new ClaimsIdentity(claims, Scheme.Name);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        return AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name));
    }

    private static bool AllowAnonymousAccess(HttpContext context)
        => context.GetEndpoint()?.Metadata?.GetMetadata<IAllowAnonymous>() is not null;
}