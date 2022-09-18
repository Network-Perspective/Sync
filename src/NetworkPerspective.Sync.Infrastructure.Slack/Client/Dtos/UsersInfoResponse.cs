using Newtonsoft.Json;

namespace NetworkPerspective.Sync.Infrastructure.Slack.Client.Dtos
{
    internal class UsersInfoResponse : IResponseWithError
    {
        [JsonProperty("ok")]
        public bool IsOk { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("user")]
        public SingleUser User { get; set; }

        [JsonProperty("response_metadata")]
        public MetadataResponse Metadata { get; set; }// = MetadataResponse.Empty;

        internal class SingleUser
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("profile")]
            public Profile Profile { get; set; }
        }

        internal class Profile
        {
            [JsonProperty("email")]
            public string Email { get; set; }
        }
    }
}