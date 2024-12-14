using System;
using System.Threading.Tasks;

namespace NetworkPerspective.Sync.Contract.V1;

public class OrchestratorClientConfiguration
{
    public Func<Task<string>> TokenFactory { get; set; }
}