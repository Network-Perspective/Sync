﻿using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Exceptions;

public class EntityNotFoundException : DbException
{
    public EntityNotFoundException(Type type) : base($"Entity of type '{type}' cannot be found")
    {
    }
}