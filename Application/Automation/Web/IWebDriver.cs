namespace Application.Automation.Web;

public interface IWebDriver : IAutomationDriver, IAsyncDisposable
{
    IWebPage Page { get; }

    Task<IWebSession> StartSessionAsync(string name, WebContextOptions? options = null);

    IWebSession GetSession(string name);
}
