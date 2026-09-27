using Microsoft.Playwright;

using Application.Elements;

using SharedKernel.Locators;

namespace Infrastructure.Automation.Web;

public sealed class WebResolvedElement : IResolvedElement
{
    public WebResolvedElement(Locator locator, ILocator handle)
    {
        Locator = locator;
        Handle = handle;
    }

    public Locator Locator { get; }

    public ILocator Handle { get; }
}
