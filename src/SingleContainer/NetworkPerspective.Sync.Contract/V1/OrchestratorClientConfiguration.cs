using System;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Contract.V1.Dtos;

namespace NetworkPerspective.Sync.Contract.V1;

public class OrchestratorClientConfiguration
{
    public Func<StartSyncDto, Task<SyncCompletedDto>> OnStartSync { get; set; }
    public Func<SetSecretsDto, Task> OnSetSecret { get; set; }
}