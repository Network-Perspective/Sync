using System.Collections.Generic;

using Microsoft.Extensions.Logging;

using NetworkPerspective.Sync.Utils.Models;
using NetworkPerspective.Sync.Worker.Application.Domain.Interactions.Criterias;

namespace NetworkPerspective.Sync.Worker.Application.Services;

public interface IInteractionsFilterFactory
{
    IInteractionsFilter CreateInteractionsFilter(TimeRange timeRange);
}

internal class InteractionsFilterFactory : IInteractionsFilterFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public InteractionsFilterFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public IInteractionsFilter CreateInteractionsFilter(TimeRange timeRange)
    {
        var timeRangeCriteria = new TimeRangeInteractionCriteria(timeRange, _loggerFactory.CreateLogger<TimeRangeInteractionCriteria>());
        var nonBotCriteria = new NonBotInteractionCriteria(_loggerFactory.CreateLogger<NonBotInteractionCriteria>());
        var nonSelfCriteria = new NonSelfInteractionCriteria(_loggerFactory.CreateLogger<NonSelfInteractionCriteria>());
        var nonExternalToExternalCriteria = new NonExternalToExternalCriteria(_loggerFactory.CreateLogger<NonExternalToExternalCriteria>());

        var criterias = new List<IInteractionCritieria> { timeRangeCriteria, nonBotCriteria, nonSelfCriteria, nonExternalToExternalCriteria };

        return new InteractionsFilter(criterias, _loggerFactory.CreateLogger<InteractionsFilter>());
    }
}