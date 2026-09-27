using Domain.Events.EventsRegistry;
using Domain.Events;

using SharedKernel.Attributes;

namespace Infrastructure.Runtime.EventsService;

public class EventService: IEventRegistry
{

    private readonly Dictionary<string, ITestEvent> _events;

    public EventService(IEnumerable<ITestEvent> events)
    {
        _events = events.ToDictionary(
            x => x.GetType()
            .GetCustomAttributes(typeof(ActionAttribute), false)
            .Cast<ActionAttribute>()
            .Single()
            .Name,
            StringComparer.OrdinalIgnoreCase);
    }

    public ITestEvent Get(string action)
    {
        if (!_events.TryGetValue(action, out var testEvent))
        {
            throw new InvalidOperationException(
                $"No event registered for action '{action}'"
            );
        }

        return testEvent;
    }
}
