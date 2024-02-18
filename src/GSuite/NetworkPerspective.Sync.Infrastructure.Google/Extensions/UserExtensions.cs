using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Google.Apis.Admin.Directory.directory_v1.Data;

using NetworkPerspective.Sync.Application.Domain.Employees;

using Newtonsoft.Json.Linq;

using DomainGroup = NetworkPerspective.Sync.Application.Domain.Employees.Group;

namespace NetworkPerspective.Sync.Infrastructure.Google.Extensions
{
    public static class UserExtensions
    {
        public static string GetFullName(this User user)
            => user.Name?.FullName ?? string.Empty;

        public static string GetManagerEmail(this User user)
        {
            var manager = user.Relations?.FirstOrDefault(r => string.Equals(r.Type, "manager", StringComparison.InvariantCultureIgnoreCase))?.Value;

            return manager ?? string.Empty;
        }

        public static DateTime? GetAccountCreationDate(this User user)
            => user.CreationTimeDateTimeOffset?.UtcDateTime;

        public static ISet<string> GetOrganizationGroupsIds(this User user)
        {
            if (string.IsNullOrEmpty(user.OrgUnitPath))
                return ImmutableHashSet<string>.Empty;

            const string pathSeparator = "/";

            var result = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            result.Add(pathSeparator);

            var groupNames = user.OrgUnitPath
                .Split(pathSeparator, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < groupNames.Length; i++)
            {
                var groupId = pathSeparator + string.Join(pathSeparator, groupNames.Take(i + 1));
                result.Add(groupId);
            }

            return result;
        }

        public static IEnumerable<DomainGroup> GetDepartmentGroups(this User user)
        {
            var result = new List<DomainGroup>();

            if (user.Organizations != null)
            {
                var departments = user.Organizations
                    .Select(x => x.Department)
                    .Where(x => x != null);

                var groups = departments.Select(x => DomainGroup.Create($"Department:{x}", x, DomainGroup.DepartmentCatergory));

                result.AddRange(groups);
            }

            return result;
        }

        public static IEnumerable<CustomAttr> GetCustomAttrs(this User user)
        {
            if (user.CustomSchemas is null)
                return Array.Empty<CustomAttr>();

            var flatDict = FlattenDict(user.CustomSchemas);

            var result = new List<CustomAttr>();

            foreach (var entry in flatDict)
            {
                if (entry.Value.GetType() == typeof(JArray))
                {
                    var values = ((JArray)entry.Value).ToObject<List<MultiValueCustomField>>();
                    result.AddRange(values.Select(x => CustomAttr.CreateMultiValue(entry.Key, x.Value)));
                }
                else
                {
                    result.Add(CustomAttr.Create(entry.Key, entry.Value));
                }
            }

            return result;
        }

        private static IDictionary<string, object> FlattenDict(IDictionary<string, IDictionary<string, object>> input)
        {
            var result = new Dictionary<string, object>();

            foreach (var section in input)
            {
                foreach (var attribute in section.Value)
                {
                    var name = $"{section.Key}.{attribute.Key}";
                    result.Add(name, attribute.Value);
                }
            }

            return result;
        }

        class MultiValueCustomField
        {
            public object Value { get; set; }
            public string Type { get; set; }
            public string CustomType { get; set; }
        }
    }
}