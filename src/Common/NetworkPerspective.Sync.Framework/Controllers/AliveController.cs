using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;

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
        /// <response code="200">Alive</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetAlive()
            => Ok();
    }
}