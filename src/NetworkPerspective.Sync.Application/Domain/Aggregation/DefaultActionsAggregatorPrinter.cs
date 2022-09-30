using System.Linq;
using System.Text;

namespace NetworkPerspective.Sync.Application.Domain.Aggregation
{
    public class DefaultActionsAggregatorPrinter
    {
        public const string DateTimeFormat = "dd.MM.yyyy";

        public string Print(ActionsAggregator aggregator)
        {
            var messagesPerDay = aggregator.GetActionsPerDay();
            var sb = new StringBuilder();

            if (messagesPerDay.Any())
            {
                sb.AppendLine($"Subject '{aggregator.Subject}' has:");

                foreach (var entry in messagesPerDay)
                    sb.AppendLine($"{entry.Value} action/s at {entry.Key.ToString(DateTimeFormat)}");
            }
            else
            {
                sb.AppendLine($"Subject '{aggregator.Subject}' has no actions");
            }

            return sb.ToString();
        }
    }
}