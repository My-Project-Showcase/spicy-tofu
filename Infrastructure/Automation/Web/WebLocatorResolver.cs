using Application.Automation.Web;
using Application.Elements;
using Application.Locators;

using SharedKernel.Elements;

namespace Infrastructure.Automation.Web;

public sealed class WebLocatorResolver : ILocatorResolver
{
    private readonly IWebDriver _driver;

    public WebLocatorResolver(IWebDriver driver)
    {
        _driver = driver;
    }

    public async Task<IResolvedElement> ResolveAsync(Element element)
    {
        foreach (var locator in element.Locators)
        {
            var resolved = await _driver.Page.ResolveAsync(locator);
            if (resolved is not null)
            {
                return resolved;
            }
        }

        throw new InvalidOperationException($"No locator resolved for element '{element.Name}'.");
    }
}
