using System;
using System.Text;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Infrastructure.Vaults.Contract.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Orchestrator.Application.Infrastructure.Persistence.Exceptions;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services;

public interface IErrorService
{
    Error MapToError(Exception exception);
}

public class ErrorService(ILogger<ErrorService> logger) : IErrorService
{
    private readonly ILogger<ErrorService> _logger = logger;

    public Error MapToError(Exception exception)
    {
        if (exception.IndicatesTaskCanceled())
        {
            _logger.LogInformation("Operation cancelled");
            return new Error(
                type: Error.Types.Application,
                title: "Operation Cancelled",
                details: "The operation has been canceled before completed",
                statusCode: 400);
        }

        _logger.LogError(exception, "Application has thrown an exception");

        switch (exception)
        {
            case MissingAuthorizationHeaderException mahex:
                {
                    return new Error(
                        type: Error.Types.Security,
                        title: "Authentication Error",
                        details: mahex.Message,
                        statusCode: 401);
                }
            case OAuthException aex:
                {
                    var details = new StringBuilder();
                    details.Append(aex.Error);

                    if (!string.IsNullOrEmpty(aex.ErrorDescription))
                        details.Append($" ({aex.ErrorDescription})");

                    return new Error(
                        type: Error.Types.Security,
                        title: "OAuth error",
                        details: details.ToString(),
                        statusCode: 400);
                }
            case VaultException vex:
                {
                    return new Error(
                        type: Error.Types.SecretStorage,
                        title: "Vault Interface Error",
                        details: vex.Message,
                        statusCode: 500);
                }
            case DbException dbex:
                {
                    return new Error(
                        type: Error.Types.Database,
                        title: "Database Error",
                        details: dbex.Message,
                        statusCode: 500);
                }
            default:
                {
                    return new Error(
                        type: Error.Types.Unknown,
                        title: "Unexpected Error",
                        details: "Unexpected exception has been thrown. Please see logs for details",
                        statusCode: 500);
                }
        }
    }
}