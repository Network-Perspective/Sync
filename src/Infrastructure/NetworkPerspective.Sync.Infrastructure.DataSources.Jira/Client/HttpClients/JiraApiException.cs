using System;

using NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.Dtos;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Client.HttpClients;

public class JiraApiException : Exception
{
    public ErrorResponse ErrorResponse { get; }

    public JiraApiException(ErrorResponse errorResponse)
    {
        ErrorResponse = errorResponse;
    }
}