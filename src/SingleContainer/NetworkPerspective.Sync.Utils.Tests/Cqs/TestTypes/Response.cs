﻿using System;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Utils.Tests.Cqs.TestTypes;

internal class Response : IResponse
{
    public Guid CorrelationId { get; set; }
}