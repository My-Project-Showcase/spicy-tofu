using Microsoft.Playwright;

using Application.Automation.Web;

namespace Infrastructure.Automation.Web;

public sealed class BrowserSession : IWebSession, IAsyncDisposable
{
    public BrowserSession(string name, IBrowserContext context, IPage page)
    {
        Name = name;
        Context = context;
        Page = new WebPage(page);
    }

    public string Name { get; }

    public IWebPage Page { get; }

    public IBrowserContext Context { get; }

    public async ValueTask DisposeAsync()
    {
        await Context.CloseAsync();
    }
}