using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkPerspective.Sync.Application.Domain.Employees
{
    public class EmployeeCollection
    {
        public bool IsHashed => _hashFunc != null;

        private readonly IDictionary<string, Employee> _emailLookupTable = new Dictionary<string, Employee>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Func<string, string> _hashFunc;

        public EmployeeCollection(Func<string, string> hashFunc)
        {
            _hashFunc = hashFunc;
        }

        public void Add(Employee employee, ISet<string> aliases)
        {
            var employeeToInsert = _hashFunc == null ? employee : employee.Hash(_hashFunc);
            var aliasesToUse = _hashFunc == null ? aliases : aliases.Select(_hashFunc);

            AddIfNotExists(employeeToInsert.Email, employeeToInsert);
            AddIfNotExists(employeeToInsert.SourceInternalId, employeeToInsert);

            foreach (var alias in aliasesToUse)
                AddIfNotExists(alias, employeeToInsert);
        }

        private void AddIfNotExists(string alias, Employee employee)
        {
            if (!_emailLookupTable.ContainsKey(alias) && !string.IsNullOrEmpty(alias))
                _emailLookupTable.Add(alias, employee);
        }

        public IEnumerable<Employee> GetAllInternal()
            => _emailLookupTable.Values
            .Where(x => x.IsBot == false)
            .ToHashSet(new EmployeeEqualityComparer());

        public Employee Find(string alias)
        {
            return IsInternal(alias)
                ? _emailLookupTable[alias]
                : Employee.CreateExternal(alias);
        }

        private bool IsInternal(string alias)
            => _emailLookupTable.ContainsKey(alias);

        public void EvaluateHierarchy()
        {
            var employees = GetAllInternal();

            foreach (var employee in employees)
            {
                if (!HasAnySubordinates(employee, employees))
                    employee.SetHierarchy(EmployeeHierarchy.IndividualContributor);
                else
                    employee.SetHierarchy(EmployeeHierarchy.Manager);
            }

            foreach (var employee in employees)
            {
                var subordinates = GetSubordinates(employee, employees);

                if (subordinates.Any(x => x.GetHierarchy() == EmployeeHierarchy.Manager))
                    employee.SetHierarchy(EmployeeHierarchy.Director);

                if (string.IsNullOrEmpty(employee.ManagerEmail))
                    employee.SetHierarchy(EmployeeHierarchy.Board);
            }
        }

        private bool HasAnySubordinates(Employee employee, IEnumerable<Employee> employees)
            => GetSubordinates(employee, employees).Any();

        private IEnumerable<Employee> GetSubordinates(Employee employee, IEnumerable<Employee> allEmployees)
            => allEmployees
                .Where(x => x.ManagerEmail is not null)
                .Where(x => Find(x.ManagerEmail).Email == employee.Email);
    }
}