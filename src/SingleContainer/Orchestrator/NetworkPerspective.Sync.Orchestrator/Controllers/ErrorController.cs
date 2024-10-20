using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Orchestrator.Application.Services;

namespace NetworkPerspective.Sync.Orchestrator.Controllers;

[ApiController]
[ApiExplorerSettings(IgnoreApi = true)]
public class ErrorController(IErrorService errorService) : ControllerBase
{
    public const string ErrorRoute = "/error";

    [Route(ErrorRoute)]
    [ApiExplorerSettings(IgnoreApi = true)]
    public ProblemDetails HandleException()
    {
        var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

        var error = errorService.MapToError(context.Error);

        Response.StatusCode = error.StatusCode;

        return new ProblemDetails
        {
            Type = error.Type,
            Title = error.Title,
            Detail = error.Details,
            Status = error.StatusCode
        };
    }
}