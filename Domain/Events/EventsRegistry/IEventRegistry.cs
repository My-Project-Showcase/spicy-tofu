namespace Domain.Events.EventsRegistry;

public interface IEventRegistry
{
    ITestEvent Get(string action);
}
