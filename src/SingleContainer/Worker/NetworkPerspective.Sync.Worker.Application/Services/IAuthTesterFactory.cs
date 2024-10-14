using System;

using Microsoft.Extensions.DependencyInjection;

using NetworkPerspective.Sync.Worker.Application.Exceptions;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IAuthTesterFactory
{
    IAuthTester CreateAuthTester();
}

internal class AuthTesterFactory(IAuthTesterContextAccessor contextAccessor, IServiceProvider serviceProvider) : IAuthTesterFactory
{
    public IAuthTester CreateAuthTester()
    {
        var type = contextAccessor.Context.ConnectorType;

        return ConnectorTypeToAuthTester(type);
    }

    private IAuthTester ConnectorTypeToAuthTester(string connectorType)
    {
        if (string.Equals(connectorType, "Slack"))
            return serviceProvider.GetRequiredKeyedService<IAuthTester>("NetworkPerspective.Sync.Infrastructure.DataSources.Slack.Services.AuthTester");
        else if (string.Equals(connectorType, "Google"))
            return serviceProvider.GetRequiredKeyedService<IAuthTester>("NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services.AuthTester");
        else if (string.Equals(connectorType, "Excel"))
            return serviceProvider.GetRequiredKeyedService<IAuthTester>("NetworkPerspective.Sync.Infrastructure.DataSources.Excel.Services.AuthTester");
        else if (string.Equals(connectorType, "Office365"))
            return serviceProvider.GetRequiredKeyedService<IAuthTester>("NetworkPerspective.Sync.Infrastructure.DataSources.Microsoft.Services.AuthTester");
        else if (string.Equals(connectorType, "Jira"))
            return serviceProvider.GetRequiredKeyedService<IAuthTester>("NetworkPerspective.Sync.Infrastructure.DataSources.Jira.Services.AuthTester");
        else
            throw new InvalidConnectorTypeException(connectorType);
    }
}