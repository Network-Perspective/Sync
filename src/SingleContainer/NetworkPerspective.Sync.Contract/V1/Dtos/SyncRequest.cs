using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class SyncRequest : IRequest<SyncResponse>, IConnectorScoped
{
    public string UserFriendlyName { get; set; } = "Trigger Synchronization";
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public ConnectorDto Connector { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string AccessToken { get; set; }
    public IEnumerable<EmployeeDto> Employees { get; set; }
}