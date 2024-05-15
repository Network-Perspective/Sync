using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using NetworkPerspective.Sync.Contract.V1.Dtos;
using NetworkPerspective.Sync.Orchestrator.Hubs;

namespace NetworkPerspective.Sync.Orchestrator.Controllers
{
    [AllowAnonymous]
    public class TestController : ControllerBase
    {
        private readonly WorkerHubV1 _hub;

        public TestController(WorkerHubV1 hub)
        {
            _hub = hub;
        }

        [HttpGet("api/test")]
        public async Task Test()
        {
            var startRequest = new StartSyncDto
            {
                Start = new DateTime(2022, 01, 01),
                End = DateTime.UtcNow,
            };
            await _hub.StartSyncAsync("", startRequest);
        }
    }
}