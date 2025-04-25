using System;
using System.Collections.Generic;

using NetworkPerspective.Sync.Utils.CQS.Queries;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class ConnectorStatusResponse : IResponse
{
    public Guid CorrelationId { get; set; }
    public bool IsRunning { get; set; }
    public bool IsAuthorized { get; set; }
    public IEnumerable<KeyValuePair<string, string>> CustomProps { get; set; } = [];

    public string CurrentTaskCaption { get; set; }
    public string CurrentTaskDescription { get; set; }
    public double? CurrentTaskCompletionRate { get; set; }
}