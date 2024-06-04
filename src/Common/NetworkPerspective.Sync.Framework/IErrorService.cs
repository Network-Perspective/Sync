using System;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Exceptions;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Core.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.Persistence.Exceptions;
using NetworkPerspective.Sync.Application.Infrastructure.SecretStorage.Exceptions;
using NetworkPerspective.Sync.Framework.Exceptions;

namespace NetworkPerspective.Sync.Framework
{
    public interface IErrorService
    {
        Error MapToError(Exception exception);
    }

    public class ErrorService : IErrorService
    {
        private readonly ILogger<ErrorService> _logger;

        public ErrorService(ILogger<ErrorService> logger)
        {
            _logger = logger;
        }

        public Error MapToError(Exception exception)
        {
            if (exception.IndicatesTaskCanceled())
            {
                _logger.LogInformation("Operation cancelled");
                return new Error(
                    type: Error.Types.Application,
                    title: "Operation Cancelled",
                    details: "The operation has been canceled before completed",
                    statusCode: StatusCodes.Status400BadRequest);
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
                            statusCode: StatusCodes.Status401Unauthorized);
                    }
                case InvalidTokenException iatex:
                    {
                        return new Error(
                            type: Error.Types.Security,
                            title: "Authentication Error",
                            details: iatex.Message,
                            statusCode: StatusCodes.Status401Unauthorized);
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
                            statusCode: StatusCodes.Status400BadRequest);
                    }
                case SecretStorageException ssex:
                    {
                        return new Error(
                            type: Error.Types.SecretStorage,
                            title: "Secret Storage Interface Error",
                            details: ssex.Message,
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                case DbException dbex:
                    {
                        return new Error(
                            type: Error.Types.Database,
                            title: "Database Error",
                            details: dbex.Message,
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                case NetworkPerspectiveCoreException npcex:
                    {
                        return new Error(
                            type: Error.Types.NetworkPerspectiveCore,
                            title: "Network Perspective Core Interface Error",
                            details: npcex.Message,
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
                case ConnectorNotFoundException nnfex:
                    {
                        return new Error(
                            type: Error.Types.Application,
                            title: "Application Error",
                            details: $"Network '{nnfex.ConnectorId}' doesn't exist. Please add network",
                            statusCode: StatusCodes.Status404NotFound);
                    }
                case Exception ex when ex.IndicatesTaskCanceled():
                    {
                        return new Error(
                            type: Error.Types.Client,
                            title: "Client Error",
                            details: "Client cancelled the request",
                            statusCode: StatusCodes.Status400BadRequest);
                    }
                default:
                    {
                        return new Error(
                            type: Error.Types.Unknown,
                            title: "Unexpected Error",
                            details: "Unexpected exception has been thrown. Please see logs for details",
                            statusCode: StatusCodes.Status500InternalServerError);
                    }
            }
        }
    }
}