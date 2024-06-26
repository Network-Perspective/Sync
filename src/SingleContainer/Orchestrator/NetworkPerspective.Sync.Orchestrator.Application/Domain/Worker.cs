﻿using System;

namespace NetworkPerspective.Sync.Orchestrator.Application.Domain;

public class Worker
{
    public Guid Id { get; }
    public int Version { get; }
    public string Name { get; }
    public string SecretHash { get; }
    public string SecretSalt { get; }
    public bool IsAuthorized { get; private set; }
    public DateTime CreatedAt { get; }

    public Worker(Guid id, int version, string name, string secretHash, string secretSalt, bool isApproved, DateTime createdAt)
    {
        Id = id;
        Version = version;
        Name = name;
        SecretHash = secretHash;
        SecretSalt = secretSalt;
        IsAuthorized = isApproved;
        CreatedAt = createdAt;
    }

    public void Authorize()
        => IsAuthorized = true;
}