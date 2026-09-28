using OpenQA.Selenium;
using OpenQA.Selenium.Appium;

using Application.Automation.Mobile;
using Application.Elements;

using SharedKernel.Locators;

namespace Infrastructure.Automation.Mobile;

public sealed class MobileSession : IMobileSession
{
    public MobileSession(string name, AppiumDriver driver)
    {
        Name = name;
        Driver = driver;
    }

    public string Name { get; }

    public AppiumDriver Driver { get; }

    public Task<IResolvedElement?> ResolveAsync(Locator locator)
    {
        var by = TryBuildBy(locator);
        if (by is null)
        {
            return Task.FromResult<IResolvedElement?>(null);
        }

        var elements = Driver.FindElements(by);
        IResolvedElement? resolved = elements.Count > 0 ? new MobileResolvedElement(locator, elements[0]) : null;
        return Task.FromResult(resolved);
    }

    public Task NavigateAsync(string url)
    {
        Driver.Navigate().GoToUrl(url);
        return Task.CompletedTask;
    }

    private static By? TryBuildBy(Locator locator) =>
        locator.Strategy switch
        {
            LocatorStrategy.Id => MobileBy.Id(locator.Value),
            LocatorStrategy.Name => MobileBy.Name(locator.Value),
            LocatorStrategy.AccessibilityId => MobileBy.AccessibilityId(locator.Value),
            LocatorStrategy.XPath => MobileBy.XPath(locator.Value),
            _ => null
        };
}
