
namespace NetworkPerspective.Sync.Contract;


public interface IOrchestratorClient
{
    Task<AckResponseDto> RegisterConnectorAsync(RegisterConnectorRequestDto registerConnectorDto);
}

public interface IConnectorClient
{
    Task<AckResponseDto> StartSyncAsync(StartSyncRequestDto startSyncRequestDto);
}



public class RegisterConnectorRequestDto : IRequest
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
}

public class StartSyncRequestDto : IRequest
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
}



public class AckResponseDto : IResponse
{
    public Guid CorrelationId { get; set; }
}


public interface IRequest
{
    public Guid CorrelationId { get; set; }
}

public interface IResponse
{
    public Guid CorrelationId { get; set; }
}
