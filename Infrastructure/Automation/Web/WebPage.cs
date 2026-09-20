using Application.Automation.Web;

using Microsoft.Playwright;

namespace Infrastructure.Automation.Web;

public sealed class WebPage : IWebPage
{
    public WebPage(IPage page)
    {
        Page = page;
    }

    public IPage Page { get; }
}