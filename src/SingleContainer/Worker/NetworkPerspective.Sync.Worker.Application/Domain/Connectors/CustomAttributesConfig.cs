using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Connectors
{
    public class CustomAttributesConfig
    {
        public static readonly CustomAttributesConfig Empty = new CustomAttributesConfig(Array.Empty<string>(), Array.Empty<string>(), Array.Empty<CustomAttributeRelationship>());

        public IEnumerable<string> GroupAttributes { get; }
        public IEnumerable<string> PropAttributes { get; }
        public IEnumerable<CustomAttributeRelationship> Relationships { get; }

        public CustomAttributesConfig(IEnumerable<string> groupAttributes, IEnumerable<string> propAttributes, IEnumerable<CustomAttributeRelationship> relationships)
        {
            GroupAttributes = groupAttributes ?? Array.Empty<string>();
            PropAttributes = propAttributes ?? Array.Empty<string>();
            Relationships = relationships;
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            var group = GroupAttributes.Any() ? string.Join(", ", GroupAttributes) : "<empty>";
            var prop = PropAttributes.Any() ? string.Join(", ", PropAttributes) : "<empty>";
            var relationshiips = Relationships.Any() ? string.Join("; ", Relationships) : "<empty>";

            stringBuilder.AppendLine($"Group: {group};");
            stringBuilder.AppendLine($"Prop: {prop};");
            stringBuilder.AppendLine($"Relationships: {relationshiips};");

            return stringBuilder.ToString();
        }
    }
}