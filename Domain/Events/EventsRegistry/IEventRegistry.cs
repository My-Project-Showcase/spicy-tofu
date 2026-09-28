using Domain.Events;

namespace Domain.Events.EventsRegistry;

public interface IEventRegistry
{
    ITestEvent Get(string action);

    bool TryGet(string action, out ITestEvent? testEvent);
}
