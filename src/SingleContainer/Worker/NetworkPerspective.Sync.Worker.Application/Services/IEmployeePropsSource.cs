using System.Collections.Generic;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IEmployeePropsSource
{
    void AddPropForUser(string primaryId, string key, object value);
    IDictionary<string, object> EnrichProps(string primaryId, IDictionary<string, object> currentProps);
}

public class EmployeePropsSource : IEmployeePropsSource
{
    public static EmployeePropsSource Empty => new();

    private readonly Dictionary<string, Dictionary<string, object>> _props = new();

    public void AddPropForUser(string primaryId, string key, object value)
    {
        if (!_props.ContainsKey(primaryId))
            _props.Add(primaryId, new Dictionary<string, object>());

        _props[primaryId].Add(key, value);
    }

    public IDictionary<string, object> EnrichProps(string primaryId, IDictionary<string, object> currentProps)
    {
        if (!_props.ContainsKey(primaryId))
            return currentProps;

        var newProps = new Dictionary<string, object>();

        if (currentProps != null)
            newProps = new Dictionary<string, object>(currentProps);

        var props = _props[primaryId];
        foreach (var prop in props)
        {
            if (!newProps.ContainsKey(prop.Key))
                newProps.Add(prop.Key, prop.Value);
        }

        return newProps;
    }
}