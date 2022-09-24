using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace NetworkPerspective.Sync.Framework.Controllers
{
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        public const string ErrorRoute = "/error";

        private readonly IErrorService _errorService;

        public ErrorController(IErrorService errorService)
        {
            _errorService = errorService;
        }

        [Route(ErrorRoute)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public ProblemDetails HandleException()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();

            var error = _errorService.MapToError(context.Error);

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
}