using OpenQA.Selenium;

using Application.Elements;

using SharedKernel.Locators;

namespace Infrastructure.Automation.Mobile;

public sealed class MobileResolvedElement : IResolvedElement
{
    public MobileResolvedElement(Locator locator, IWebElement handle)
    {
        Locator = locator;
        Handle = handle;
    }

    public Locator Locator { get; }

    public IWebElement Handle { get; }
}
