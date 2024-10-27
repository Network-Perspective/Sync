using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Contract.V1.Dtos;

public class ConnectorDto
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public IDictionary<string, string> Properties { get; set; }
}