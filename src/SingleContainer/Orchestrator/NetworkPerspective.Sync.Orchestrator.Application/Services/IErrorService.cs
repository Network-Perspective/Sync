using System;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Orchestrator.Application.Domain;
using NetworkPerspective.Sync.Orchestrator.Application.Exceptions;
using NetworkPerspective.Sync.Utils.Extensions;

namespace NetworkPerspective.Sync.Orchestrator.Application.Services
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
                //case InvalidTokenException iatex:
                //    {
                //        return new Error(
                //            type: Error.Types.Security,
                //            title: "Authentication Error",
                //            details: iatex.Message,
                //            statusCode: StatusCodes.Status401Unauthorized);
                //    }
                //case OAuthException aex:
                //    {
                //        var details = new StringBuilder();
                //        details.Append(aex.Error);

                //        if (!string.IsNullOrEmpty(aex.ErrorDescription))
                //            details.Append($" ({aex.ErrorDescription})");

                //        return new Error(
                //            type: Error.Types.Security,
                //            title: "OAuth error",
                //            details: details.ToString(),
                //            statusCode: StatusCodes.Status400BadRequest);
                //    }
                //case SecretStorageException ssex:
                //    {
                //        return new Error(
                //            type: Error.Types.SecretStorage,
                //            title: "Secret Storage Interface Error",
                //            details: ssex.Message,
                //            statusCode: StatusCodes.Status500InternalServerError);
                //    }
                //case DbException dbex:
                //    {
                //        return new Error(
                //            type: Error.Types.Database,
                //            title: "Database Error",
                //            details: dbex.Message,
                //            statusCode: StatusCodes.Status500InternalServerError);
                //    }
                //case NetworkPerspectiveCoreException npcex:
                //    {
                //        return new Error(
                //            type: Error.Types.NetworkPerspectiveCore,
                //            title: "Network Perspective Core Interface Error",
                //            details: npcex.Message,
                //            statusCode: StatusCodes.Status500InternalServerError);
                //    }
                //case NetworkNotFoundException nnfex:
                //    {
                //        return new Error(
                //            type: Error.Types.Application,
                //            title: "Application Error",
                //            details: $"Network '{nnfex.NetworkId}' doesn't exist. Please add network",
                //            statusCode: StatusCodes.Status404NotFound);
                //    }
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
}