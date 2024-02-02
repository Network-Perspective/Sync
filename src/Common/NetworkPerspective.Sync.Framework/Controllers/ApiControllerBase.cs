using System.Security;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

using NetworkPerspective.Sync.Application.Domain;
using NetworkPerspective.Sync.Application.Extensions;
using NetworkPerspective.Sync.Application.Infrastructure.Core;
using NetworkPerspective.Sync.Application.Services;
using NetworkPerspective.Sync.Framework.Exceptions;

namespace NetworkPerspective.Sync.Framework.Controllers
{
    [ApiController]
    public class ApiControllerBase : ControllerBase
    {
        private readonly INetworkPerspectiveCore _networkPerspectiveCore;
        private readonly INetworkIdInitializer _networkIdInitializer;

        public ApiControllerBase(INetworkPerspectiveCore networkPerspectiveCore, INetworkIdInitializer networkIdInitializer)
        {
            _networkPerspectiveCore = networkPerspectiveCore;
            _networkIdInitializer = networkIdInitializer;
        }

        protected async Task<TokenValidationResponse> ValidateTokenAsync(CancellationToken stoppingToken)
        {
            var result = await _networkPerspectiveCore.ValidateTokenAsync(GetAccessToken(), stoppingToken);
            _networkIdInitializer.Initialize(result.NetworkId);
            return result;
        }

        protected SecureString GetAccessToken()
        {
            var value = Request.Headers[HeaderNames.Authorization].ToString();

            if (string.IsNullOrEmpty(value))
                throw new MissingAuthorizationHeaderException();

            return value
                .Replace("Bearer", string.Empty)
                .Trim()
                .ToSecureString();
        }
    }
}