using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Worker.Application.Domain.Connectors;

namespace NetworkPerspective.Sync.Common.Tests;

public class TestableConnectorProperties : ConnectorProperties
{
    public TestableConnectorProperties() : base(true, true, false, null)
    { }

    public string StringProp { get; set; } = string.Empty;
    public int IntProp { get; set; } = 0;
    public bool BoolProp { get; set; } = false;

    public override void Bind(IEnumerable<KeyValuePair<string, string>> properties)
    {
        base.Bind(properties);

        if (properties.Any(x => x.Key == nameof(StringProp)))
            StringProp = properties.Single(x => x.Key == nameof(StringProp)).Value;

        if (properties.Any(x => x.Key == nameof(IntProp)))
            IntProp = int.Parse(properties.Single(x => x.Key == nameof(IntProp)).Value);

        if (properties.Any(x => x.Key == nameof(BoolProp)))
            BoolProp = bool.Parse(properties.Single(x => x.Key == nameof(BoolProp)).Value);
    }

    public override IEnumerable<KeyValuePair<string, string>> GetAll()
    {
        var props = new List<KeyValuePair<string, string>>
        {
            new(nameof(StringProp), StringProp),
            new(nameof(IntProp), IntProp.ToString()),
            new(nameof(BoolProp), BoolProp.ToString())
        };

        props.AddRange(base.GetAll());

        return props;
    }
}