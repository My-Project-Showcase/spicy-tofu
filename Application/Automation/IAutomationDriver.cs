namespace Application.Automation;

/// <summary>
/// Defines the common automation capabilities that must be supported
/// by all automation drivers in the framework.
/// </summary>
public interface IAutomationDriver
{
    Task StartAsync();
    Task StopAsync();
}
