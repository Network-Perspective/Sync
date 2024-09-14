using System;
using System.Collections.Generic;
using System.Linq;

using NetworkPerspective.Sync.Worker.Application.Domain;

namespace NetworkPerspective.Sync.Worker.Application.Domain.Employees
{
    public class EmployeeCollection
    {
        public bool IsHashed => _hashFunc != null;

        private readonly IDictionary<string, Employee> _emailLookupTable = new Dictionary<string, Employee>(StringComparer.InvariantCultureIgnoreCase);
        private readonly HashFunction.Delegate _hashFunc;

        public EmployeeCollection(IEnumerable<Employee> employees, HashFunction.Delegate hashFunc)
        {
            _hashFunc = hashFunc;

            foreach (var employee in employees)
                Add(employee);

            EvaluateHierarchy();
        }

        private void Add(Employee employee)
        {
            var employeeToInsert = _hashFunc == null ? employee : employee.Hash(_hashFunc);

            AddIfNotExists(employeeToInsert.Id.PrimaryId, employeeToInsert);
            AddIfNotExists(employeeToInsert.Id.DataSourceId, employeeToInsert);

            foreach (var alias in employee.Id.Aliases)
                AddIfNotExists(alias, employeeToInsert);
        }

        private void AddIfNotExists(string alias, Employee employee)
        {
            if (!string.IsNullOrEmpty(alias))
                _emailLookupTable.TryAdd(alias, employee);
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

        private void EvaluateHierarchy()
        {
            var employees = GetAllInternal();

            // do not add hierarchy field none of the employees have subordinates
            if (!employees.Any(e => HasAnySubordinates(e, employees)))
            {
                return;
            }

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

                if (!employee.HasManager)
                    employee.SetHierarchy(EmployeeHierarchy.Board);
            }
        }

        private bool HasAnySubordinates(Employee employee, IEnumerable<Employee> employees)
            => GetSubordinates(employee, employees).Any();

        private IEnumerable<Employee> GetSubordinates(Employee employee, IEnumerable<Employee> allEmployees)
            => allEmployees
                .Where(x => x.HasManager)
                .Where(x => Find(x.ManagerEmail).Id.PrimaryId == employee.Id.PrimaryId);
    }
}