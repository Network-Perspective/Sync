using NetworkPerspective.Sync.Worker.Application.Exceptions;

namespace NetworkPerspective.Sync.Worker.Application.Mappers;

public static class ConnectorTypeMapper
{
    public static string ToDataSourceId(string connectorType)
    {
        return connectorType switch
        {
            "Slack" => "SlackId",
            "Google" => "GSuiteId",
            "Excel" => "ExcelId",
            "Office365" => "Office365Id",
            "Jira" => "JiraId",
            _ => throw new InvalidConnectorTypeException(connectorType),
        };
    }
}