﻿using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class StartSyncDto : IRequest
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public Guid ConnectorId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public IDictionary<string, string> NetworkProperties { get; set; }
}