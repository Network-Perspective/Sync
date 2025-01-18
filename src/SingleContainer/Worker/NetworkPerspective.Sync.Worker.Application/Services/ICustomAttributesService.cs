using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Worker.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface ICustomAttributesService
{
    ISet<Group> GetGroupsForHashedEmployee(IEnumerable<CustomAttr> customAttributes);
    IDictionary<string, object> GetPropsForHashedEmployee(IEnumerable<CustomAttr> customAttributes);
    IDictionary<string, object> GetPropsForEmployee(IEnumerable<CustomAttr> customAttributes);
    IList<Relation> GetRelations(IEnumerable<CustomAttr> customAttributes);
}

public class CustomAttributesService(ISyncContextAccessor syncContextAccessor) : ICustomAttributesService
{
    public ISet<Group> GetGroupsForHashedEmployee(IEnumerable<CustomAttr> customAttributes)
    {
        var result = new HashSet<Group>();

        foreach (var customAttr in customAttributes)
        {
            if (syncContextAccessor.SyncContext.NetworkConfig.CustomAttributes.GroupAttributes.Contains(customAttr.Name))
            {
                var group = Group.Create($"{customAttr.Name}:{customAttr.Value}", GetFormattedCustomAttrValue(customAttr), customAttr.Name);
                result.Add(group);
            }
        }

        return result;
    }

    public IDictionary<string, object> GetPropsForHashedEmployee(IEnumerable<CustomAttr> customAttributes)
    {
        var result = new Dictionary<string, object>();

        foreach (var propAttributeConfig in syncContextAccessor.SyncContext.NetworkConfig.CustomAttributes.PropAttributes)
        {
            var customAttr = customAttributes.FirstOrDefault(x => x.Name == propAttributeConfig);

            if (customAttr is not null)
                result.Add(customAttr.Name, GetFormattedCustomAttrValue(customAttr));

        }

        return result;
    }

    public IDictionary<string, object> GetPropsForEmployee(IEnumerable<CustomAttr> customAttributes)
    {
        var result = new Dictionary<string, object>();

        foreach (var groupAttributeGonfig in syncContextAccessor.SyncContext.NetworkConfig.CustomAttributes.GroupAttributes)
        {
            var customAttrs = customAttributes.Where(x => x.Name == groupAttributeGonfig);

            if (customAttrs.Any())
            {
                if (customAttrs.First().IsMultiValue)
                    result.Add(groupAttributeGonfig, customAttrs.Select(x => GetFormattedCustomAttrValue(x)));
                else
                    result.Add(groupAttributeGonfig, GetFormattedCustomAttrValue(customAttrs.Single()));
            }
        }

        return result;
    }

    public IList<Relation> GetRelations(IEnumerable<CustomAttr> customAttributes)
    {
        var result = new List<Relation>();

        foreach (var relationshipAttributeGonfig in syncContextAccessor.SyncContext.NetworkConfig.CustomAttributes.Relationships)
        {
            var customAttr = customAttributes.FirstOrDefault(x => x.Name == relationshipAttributeGonfig.PropName);

            if (customAttr is not null)
                result.Add(Relation.Create(relationshipAttributeGonfig.RelationshipName, customAttr.Value.ToString()));
        }

        return result;
    }

    private static string GetFormattedCustomAttrValue(CustomAttr customAttr)
    {
        if (customAttr.Value.GetType() == typeof(DateTime))
            return ((DateTime)customAttr.Value).ToString("yyyy-MM-dd");
        else
            return customAttr.Value.ToString();
    }
}