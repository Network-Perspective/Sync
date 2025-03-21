﻿using System.Threading;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface ISecretRotationService
{
    Task ExecuteAsync(CancellationToken stoppingToken = default);
}