using System;
using System.Threading.Tasks;

using NetworkPerspective.Sync.Contract.V1.Dtos;

namespace NetworkPerspective.Sync.Contract.V1;

public class OrchestratorClientConfiguration
{
    public Func<Task<string>> TokenFactory { get; set; }
    public Func<StartSyncDto, Task<SyncCompletedDto>> OnStartSync { get; set; }
    public Func<SetSecretsDto, Task> OnSetSecrets { get; set; }
    public Func<RotateSecretsDto, Task> OnRotateSecrets { get; set; }
    public Func<GetConnectorStatusDto, Task<ConnectorStatusDto>> OnGetConnectorStatus { get; set; }
    public Func<GetWorkerCapabilitiesDto, Task<WorkerCapabilitiesDto>> OnGetWorkerCapabilities { get; set; }
    public Func<InitializeOAuthRequest, Task<InitializeOAuthResponse>> OnInitializeOAuth { get; set; }
    public Func<HandleOAuthCallbackRequest, Task<AckDto>> OnHandleOAuth { get; set; }
}