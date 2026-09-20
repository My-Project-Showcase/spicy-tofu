namespace Application.Automation.Web;

public interface IWebDriver : IAsyncDisposable
{
    IWebPage Page { get; }

    Task StartAsync();

    Task StopAsync();

    Task<IWebSession> StartSessionAsync(string name, WebContextOptions? options = null);

    IWebSession GetSession(string name);
}