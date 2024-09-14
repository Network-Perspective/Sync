using System;
using System.Threading.Tasks;

using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Http;

using NetworkPerspective.Sync.Infrastructure.DataSources.Google.Extensions;

using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services
{
    internal class UnsuccessfulResponseHandler : IHttpUnsuccessfulResponseHandler
    {
        private readonly Func<Task> _callback = null;

        public UnsuccessfulResponseHandler(Func<Task> callback)
        {
            _callback = callback;
        }

        public async Task<bool> HandleResponseAsync(HandleUnsuccessfulResponseArgs args)
        {
            var content = await args.Response.Content.ReadAsStringAsync();
            var error = JsonConvert.DeserializeObject<TokenErrorResponse>(content);

            if (error.IsInvalidSignatureError())
                await _callback?.Invoke();

            return false;
        }
    }
}