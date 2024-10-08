﻿using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class SetSecretsDto : IRequest
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public IDictionary<string, string> Secrets { get; set; }
}