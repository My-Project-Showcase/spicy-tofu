namespace Application.Automation.Mobile;

public interface IMobileDriver : IAsyncDisposable
{
    Task StartAsync();

    Task StopAsync();

    Task<IMobileSession> StartSessionAsync(string name, MobileContextOptions? options = null);

    IMobileSession GetSession(string name);
}
