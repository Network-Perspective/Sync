﻿namespace NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

public class ConnectorType
{
    public string Name { get; init; }
    public string DataSourceId { get; init; }
    public string DataSourceFacadeFullName { get; init; }
}