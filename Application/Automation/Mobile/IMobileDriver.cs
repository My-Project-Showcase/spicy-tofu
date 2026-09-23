namespace Application.Automation.Mobile;

public interface IMobileDriver : IAutomationDriver, IAsyncDisposable
{
    Task<IMobileSession> StartSessionAsync(string name, MobileContextOptions? options = null);

    IMobileSession GetSession(string name);
}
