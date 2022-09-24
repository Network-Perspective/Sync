using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkPerspective.Sync.Application.Domain.Networks
{
    public class CustomAttributesConfig
    {
        public static readonly CustomAttributesConfig Empty = new CustomAttributesConfig(Array.Empty<string>(), Array.Empty<string>());

        public IEnumerable<string> GroupAttributes { get; }
        public IEnumerable<string> PropAttributes { get; }

        public CustomAttributesConfig(IEnumerable<string> groupAttributes, IEnumerable<string> propAttributes)
        {
            GroupAttributes = groupAttributes ?? Array.Empty<string>();
            PropAttributes = propAttributes ?? Array.Empty<string>();
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            var group = GroupAttributes.Any() ? string.Join(", ", GroupAttributes) : "<empty>";
            var prop = PropAttributes.Any() ? string.Join(", ", PropAttributes) : "<empty>";

            stringBuilder.AppendLine($"Group: {group};");
            stringBuilder.AppendLine($"Prop: {prop};");

            return stringBuilder.ToString();
        }
    }
}