using Microsoft.AspNetCore.Http;

using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Application.Infrastructure.Core;

using Swashbuckle.AspNetCore.Annotations;

namespace NetworkPerspective.Sync.Framework.Controllers
{
    [Route("/")]
    public class AliveController : ApiControllerBase
    {
        public AliveController(INetworkPerspectiveCore networkPerspectiveCore) : base(networkPerspectiveCore)
        { }

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