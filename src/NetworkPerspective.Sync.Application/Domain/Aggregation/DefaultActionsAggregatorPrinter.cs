using System.Linq;
using System.Text;

namespace NetworkPerspective.Sync.Application.Domain.Aggregation
{
    public class DefaultActionsAggregatorPrinter
    {
        public string Print(ActionsAggregator aggregator)
        {
            var messagesPerDay = aggregator.GetActionsPerDay();
            var sb = new StringBuilder();

            if (messagesPerDay.Any())
            {
                sb.AppendLine($"Subject '{aggregator.Subject}' has:");

                foreach (var entry in messagesPerDay)
                    sb.AppendLine($"{entry.Value} action/s at {entry.Key.ToString(Consts.DefaultDateTimeFormat)}");
            }
            else
            {
                sb.AppendLine($"Subject '{aggregator.Subject}' has no actions");
            }

            return sb.ToString();
        }
    }
}