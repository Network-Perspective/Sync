﻿using System;
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
        private readonly ConnectorHubV1 _hub;

        public TestController(ConnectorHubV1 hub)
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
            await _hub.StartSyncAsync(new Guid("04C753D8-FF9A-479C-B857-5D28C1EAF6C1"), startRequest);
        }
    }
}