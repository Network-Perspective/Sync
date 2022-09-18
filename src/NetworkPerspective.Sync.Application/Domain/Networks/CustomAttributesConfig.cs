using System;
using System.Collections.Generic;

namespace NetworkPerspective.Sync.Application.Domain.Networks
{
    public class CustomAttributesConfig
    {
        public static readonly CustomAttributesConfig Empty = new CustomAttributesConfig(Array.Empty<string>(), Array.Empty<string>(), Array.Empty<string>());

        public IEnumerable<string> GroupAttributes { get; set; }
        public IEnumerable<string> PropAttributes { get; set; }
        public IEnumerable<string> PathAttributes { get; set; }

        public CustomAttributesConfig(IEnumerable<string> groupAttributes, IEnumerable<string> propAttributes, IEnumerable<string> pathAttributes)
        {
            GroupAttributes = groupAttributes;
            PropAttributes = propAttributes;
            PathAttributes = pathAttributes;
        }
    }
}