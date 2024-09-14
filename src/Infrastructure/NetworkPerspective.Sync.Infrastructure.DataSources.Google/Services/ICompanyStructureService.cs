using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Worker.Application.Domain.Employees;

namespace NetworkPerspective.Sync.Infrastructure.DataSources.Google.Services
{
    internal interface ICompanyStructureService
    {
        public ISet<Group> CreateGroups(IEnumerable<string> unitsPahts);
    }

    internal class CompanyStructureService : ICompanyStructureService
    {
        public ISet<Group> CreateGroups(IEnumerable<string> unitsPaths)
        {
            var result = new HashSet<Group>(Group.EqualityComparer);

            var leafsPaths = GetLeafsPaths(unitsPaths);

            foreach (var leafPath in leafsPaths)
            {
                var groups = GetOrganizationGroups(leafPath);
                result.UnionWith(groups);
            }

            return result;
        }

        private static ISet<string> GetLeafsPaths(IEnumerable<string> unitsPaths)
        {
            var result = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            var uniquePaths = new HashSet<string>(unitsPaths, StringComparer.InvariantCultureIgnoreCase);

            foreach (var path in uniquePaths)
            {
                var allPathsButThis = uniquePaths.Where(x => x != path);

                if (!allPathsButThis.Any(x => x.StartsWith(path)))
                    result.Add(path);
            }

            return result;
        }

        private static IEnumerable<Group> GetOrganizationGroups(string unitPath)
        {
            if (string.IsNullOrEmpty(unitPath))
                return Array.Empty<Group>();

            const string PathSeparator = "/";

            var result = new List<Group>();

            var parentGroup = Group.Create(PathSeparator, PathSeparator, Group.CompanyCatergory);
            result.Add(parentGroup);

            var groupNames = unitPath
                .Split(PathSeparator, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < groupNames.Length; i++)
            {
                var groupId = PathSeparator + string.Join(PathSeparator, groupNames.Take(i + 1));
                var groupName = string.IsNullOrEmpty(groupNames[i]) ? PathSeparator : groupNames[i];
                var category = i == groupNames.Length - 1 ? Group.TeamCatergory : $"OrgUnitLevel{i + 1}";

                var group = Group.CreateWithParentId(groupId, groupName, category, parentGroup.Id);
                result.Add(group);
                parentGroup = group;
            }

            return result;
        }
    }
}