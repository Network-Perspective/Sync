using System;
using System.Collections.Generic;
using System.Linq;

namespace NetworkPerspective.Sync.Application.Domain.Aggregation
{
    public class ActionsAggregator
    {
        private readonly IDictionary<DateTime, int> _actionsPerDay = new Dictionary<DateTime, int>();

        public string Subject { get; }

        public ActionsAggregator(string subject)
        {
            Subject = subject;
        }

        public void Add(DateTime dateTime)
        {
            var day = dateTime.Date;

            if (_actionsPerDay.ContainsKey(day))
                _actionsPerDay[day] = _actionsPerDay[day] + 1;
            else
                _actionsPerDay.Add(day, 1);
        }

        public IDictionary<DateTime, int> GetActionsPerDay()
            => _actionsPerDay.ToDictionary(x => x.Key, y => y.Value);
    }
}