using System;

using NetworkPerspective.Sync.Worker.Application.Domain.Statuses;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IAuthTesterContextAccessor
{
    public bool IsAvailable { get; }
    public AuthTesterContext Context { get; set; }
}

internal class AuthTesterContextAccessor : IAuthTesterContextAccessor
{
    private readonly object _syncRoot = new();
    private AuthTesterContext _context = null;

    public bool IsAvailable
    {
        get
        {
            lock (_syncRoot)
                return _context is not null;
        }
    }

    public AuthTesterContext Context
    {
        get
        {
            lock (_syncRoot)
            {
                if (_context is null)
                    throw new NullReferenceException("Sync context is not set");

                return _context;
            }
        }
        set
        {
            lock (_syncRoot)
            {
                if (_context is not null)
                    throw new ArgumentException("Sync context is already set");

                _context = value;
            }
        }
    }
}