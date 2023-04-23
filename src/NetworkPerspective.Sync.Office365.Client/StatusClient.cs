using System.Linq;
using System.Reflection;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace NetworkPerspective.Sync.Office365.Client
{
    public partial class StatusClient
    {
        partial void UpdateJsonSerializerSettings(JsonSerializerSettings settings)
        {
            settings.ContractResolver = new CustomContractResolver();
        }

        class CustomContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var currentTaskMemberInfo = typeof(StatusDto)
                    .GetMember(nameof(StatusDto.CurrentTask))
                    .Single();

                if (member.HasSameMetadataDefinitionAs(currentTaskMemberInfo))
                {
                    var jsonProp = base.CreateProperty(member, memberSerialization);
                    jsonProp.Required = Required.Default;
                    return jsonProp;
                }

                return base.CreateProperty(member, memberSerialization);
            }
        }
    }
}