using Microsoft.Playwright;

using Application.Automation.Web;

namespace Infrastructure.Automation.Web;

public sealed class WebPage : IWebPage
{
    public WebPage(IPage page)
    {
        Page = page;
    }

    public IPage Page { get; }
}