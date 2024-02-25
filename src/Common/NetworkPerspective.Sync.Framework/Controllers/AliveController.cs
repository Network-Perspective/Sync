using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace NetworkPerspective.Sync.Framework.Controllers
{
    [Route("/")]
    [AllowAnonymous]
    public class AliveController : ControllerBase
    {
        /// <summary>
        /// Alive endpoint
        /// </summary>
        /// <returns>Ok</returns>
        [HttpGet]
        [SwaggerResponse(StatusCodes.Status200OK, "Alive")]
        public IActionResult GetAlive()
            => Ok();
    }
}